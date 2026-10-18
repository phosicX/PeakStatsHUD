using System;
using System.Collections.Generic;
using System.Reflection;
using Peak;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeakStatsEx.Compatibility;

internal enum PllStatus { NotApplicable, Pending, Ready, Unsupported }
internal enum PllFeature { Context, Route, Variant, Hazard, Campfire, PeakCampfire, Portal }

internal sealed class PllContext
{
    internal PllContext(object run, long identity, string epoch, int revision, Array slots,
        object? slot, int index, MapHandler.MapSegment segment, object? occurrence, bool isNadir)
    {
        Run = run; Identity = identity; Epoch = epoch; Revision = revision; Slots = slots;
        Slot = slot; Index = index; Segment = segment; Occurrence = occurrence; IsNadir = isNadir;
    }

    internal object Run { get; }
    internal long Identity { get; }
    internal string Epoch { get; }
    internal int Revision { get; }
    internal Array Slots { get; }
    internal object? Slot { get; }
    internal int Index { get; }
    internal MapHandler.MapSegment Segment { get; }
    internal object? Occurrence { get; }
    internal bool IsNadir { get; }
    internal bool IsVoidController => Slot == null;
}

internal sealed class PllRouteProjection
{
    internal PllRouteProjection(MapHandler.MapSegment[] segments,
        MountainProgressHandler.ProgressPoint[] points, string?[] nativeTitleKeys,
        MountainProgressHandler.ProgressPoint? peak, int peakSlot)
    {
        Segments = segments; Points = points; NativeTitleKeys = nativeTitleKeys; Peak = peak; PeakSlot = peakSlot;
    }
    internal MapHandler.MapSegment[] Segments { get; }
    internal MountainProgressHandler.ProgressPoint[] Points { get; }
    internal string?[] NativeTitleKeys { get; }
    internal MountainProgressHandler.ProgressPoint? Peak { get; }
    internal int PeakSlot { get; }
}

// Only metadata and warning keys outlive a scene. Runtime objects belong to this reader.
internal sealed partial class PLLCompatibility
{
    private const string PluginGuid = "af.pll.library";
    private static readonly Dictionary<Assembly, PllContract> Contracts = new();
    private static readonly Dictionary<Assembly, HashSet<PllFeature>> Warnings = new();
    private readonly HashSet<PllFeature> failedFeatures = new();
    private PllContract? contract;
    private string pluginVersion = "";
    private MapHandler? cachedHandler;
    private MapHandler.MapSegment? cachedSegment;
    private MapHandler.MapSegment[]? cachedSegments;
    private Scene cachedScene;
    private bool cachedInNadir;
    private float nextContextCheck;
    private object? currentRun;
    private PllStatus status;
    private PllContext? context;

    internal static bool Enabled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(PluginGuid);

    internal PllStatus ReadContext(MapHandler? handler, out PllContext? result)
    {
        result = null;
        if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(PluginGuid, out var plugin))
            return PllStatus.NotApplicable;
        if (plugin.Instance == null) return PllStatus.Pending;

        Assembly assembly = plugin.Instance.GetType().Assembly;
        Scene scene = SceneManager.GetActiveScene();
        MapHandler.MapSegment? segment = handler != null && handler.segments != null &&
            handler.currentSegment >= 0 && handler.currentSegment < handler.segments.Length
                ? handler.segments[handler.currentSegment] : null;
        if (contract?.Assembly == assembly && ReferenceEquals(handler, cachedHandler) &&
            ReferenceEquals(segment, cachedSegment) && ReferenceEquals(handler?.segments, cachedSegments) &&
            cachedScene == scene && cachedInNadir == (handler != null && handler.inNadir) &&
            Time.unscaledTime < nextContextCheck)
        {
            result = context;
            return status;
        }

        if (contract?.Assembly != assembly)
        {
            if (!Contracts.TryGetValue(assembly, out contract))
                Contracts.Add(assembly, contract = new PllContract(assembly));
            failedFeatures.Clear();
            currentRun = null;
        }
        pluginVersion = plugin.Metadata.Version.ToString();
        cachedHandler = handler; cachedSegment = segment; cachedSegments = handler?.segments;
        cachedScene = scene; cachedInNadir = handler != null && handler.inNadir;
        nextContextCheck = Time.unscaledTime + 1f;
        context = null;
        status = ReadCurrentContext(handler, scene, segment);
        result = context;
        return status;
    }

    private PllStatus ReadCurrentContext(MapHandler? handler, Scene scene, MapHandler.MapSegment? segment)
    {
        return Read(PllFeature.Context, () =>
        {
            object? run = Get("Map", "CurrentRun", null, "MapRun");
            if (!ReferenceEquals(run, currentRun))
            {
                currentRun = run;
                failedFeatures.Clear();
                routeProjection = null;
                hazardContext = null;
            }
            if (run == null) return PllStatus.NotApplicable;
            if (handler == null || !MapHandler.ExistsAndInitialized || handler.gameObject.scene != scene || segment == null)
                return PllStatus.Pending;
            if (!ReferenceEquals(Get<MapHandler>("MapRun", "Handler", run), handler) ||
                !Get<bool>("MapRun", "Active", run) || !Get<bool>("MapRun", "HasPublishedLayout", run))
                return PllStatus.Pending;

            if (Get("MapRun", "Slots", run, "MapSlot[]") is not Array slots)
                return Fail(PllFeature.Context, "Published Slots is null.");
            object? slot = Get("MapRun", "ReadyCurrentSlot", run, "MapSlot");
            bool controller = slot == null && handler.currentSegment >= slots.Length &&
                handler.inNadir && VoidBiome.instance != null &&
                ReferenceEquals(segment, VoidBiome.instance.segment);
            if (slot == null && !controller) return PllStatus.Pending;
            int index = slot == null ? -1 : Get<int>("MapSlot", "Index", slot);
            if (slot != null && (index < 0 || index >= slots.Length ||
                !ReferenceEquals(slots.GetValue(index), slot) ||
                !ReferenceEquals(Get<MapHandler.MapSegment>("MapSlot", "Segment", slot), segment) ||
                Get<bool>("MapSlot", "Released", slot)))
                return Fail(PllFeature.Context, "ReadyCurrentSlot does not own the current physical segment.");

            string epoch = Get<string>("MapRun", "GenerationEpoch", run);
            if (string.IsNullOrEmpty(epoch)) return Fail(PllFeature.Context, "Published GenerationEpoch is empty.");
            object? occurrence = slot == null ? null : Get("MapSlot", "Occurrence", slot, "FeatureOccurrence");
            if (segment.segmentParent == null || segment.segmentParent.scene != scene) return PllStatus.Pending;
            if (slot != null && (occurrence == null ||
                !ReferenceEquals(Get("FeatureOccurrence", "Run", occurrence, "MapRun"), run)))
                return Fail(PllFeature.Context, "The current occurrence belongs to another run.");
            context = new PllContext(run, Get<long>("MapRun", "Identity", run), epoch,
                Get<int>("MapRun", "LayoutRevision", run), slots, slot, index, segment,
                occurrence,
                controller || slot != null && contract!.Type("NadirLayoutSegmentSource").IsInstanceOfType(
                    Get("MapSlot", "Source", slot, "LayoutSegmentSource")));
            return PllStatus.Ready;
        });
    }

    internal bool TryGetMapSeed(MapHandler? handler, out int seed)
    {
        seed = default;
        if (ReadContext(handler, out _) != PllStatus.Ready || handler == null) return false;
        LevelGeneration generation = handler.GetComponent<LevelGeneration>();
        if (generation == null) return false;
        seed = generation.seed;
        return true;
    }

    private PllStatus Read(PllFeature feature, Func<PllStatus> query)
    {
        if (failedFeatures.Contains(feature)) return PllStatus.Unsupported;
        if (contract!.Failures.TryGetValue(feature, out string failure)) return Fail(feature, failure);
        try { return query(); }
        catch (PllContractException exception)
        {
            contract.Failures[feature] = exception.Message;
            return Fail(feature, exception.Message);
        }
        catch (TargetInvocationException exception)
        {
            return Fail(feature, $"Reflected accessor failed: {exception.InnerException?.Message ?? exception.Message}");
        }
    }

    private PllStatus Fail(PllFeature feature, string reason)
    {
        failedFeatures.Add(feature);
        Assembly assembly = contract!.Assembly;
        if (!Warnings.TryGetValue(assembly, out var warned)) Warnings.Add(assembly, warned = new());
        if (warned.Add(feature))
        {
            string result = feature switch
            {
                PllFeature.Context => "PLL map information is hidden",
                PllFeature.Route => "PLL route and variants are hidden",
                PllFeature.Variant => "PLL variants are hidden",
                PllFeature.Hazard => "PLL hazard statistics are hidden",
                PllFeature.Campfire or PllFeature.PeakCampfire => "only the next progress point remains a target candidate",
                _ => "the Nadir target is hidden",
            };
            Plugin.Logger.LogWarning($"PLL {pluginVersion} {feature}: {reason} {result}.");
        }
        return PllStatus.Unsupported;
    }

    private T Get<T>(string type, string member, object? owner) =>
        (T)contract!.Get(type, member, owner, typeof(T))!;
    private object? Get(string type, string member, object? owner, string expected) =>
        contract!.Get(type, member, owner, contract.Type(expected));

    private sealed class PllContractException : Exception
    {
        internal PllContractException(string message) : base(message) { }
    }

    private sealed class PllContract
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        private readonly Dictionary<string, MemberInfo?> members = new();
        private readonly Dictionary<string, Type> types = new();
        private bool? hasPeakProgress;
        internal PllContract(Assembly assembly) { Assembly = assembly; }
        internal Assembly Assembly { get; }
        internal Dictionary<PllFeature, string> Failures { get; } = new();
        internal MethodInfo? ResolveBehavior { get; private set; }
        internal bool HasPeakProgress => hasPeakProgress ??=
            Type("ProgressProjection+ProgressState").GetField("Peak", Flags) != null ||
            Type("ProgressProjection+ProgressState").GetProperty("Peak", Flags) != null;

        internal Type Type(string name)
        {
            if (types.TryGetValue(name, out Type type)) return type;
            type = name.EndsWith("[]", StringComparison.Ordinal)
                ? Type(name.Substring(0, name.Length - 2)).MakeArrayType()
                : Assembly.GetType("PLL.Library." + name) ??
                    throw new PllContractException($"Missing type PLL.Library.{name}.");
            types.Add(name, type);
            return type;
        }

        internal object? Get(string type, string name, object? owner, Type expected)
        {
            string key = type + "." + name;
            if (!members.TryGetValue(key, out MemberInfo? member))
            {
                Type declaring = Type(type);
                member = (MemberInfo?)declaring.GetProperty(name, Flags) ?? declaring.GetField(name, Flags);
                members.Add(key, member);
            }
            bool valid = member switch
            {
                PropertyInfo p => p.GetGetMethod(true) is { } getter && getter.IsStatic == (owner == null) &&
                    p.GetIndexParameters().Length == 0 && expected.IsAssignableFrom(p.PropertyType),
                FieldInfo f => f.IsStatic == (owner == null) && expected.IsAssignableFrom(f.FieldType),
                _ => false,
            };
            if (!valid) throw new PllContractException($"Missing or incompatible {key} ({expected.Name}).");
            return member is PropertyInfo property ? property.GetValue(owner) : ((FieldInfo)member!).GetValue(owner);
        }

        internal object? Resolve(object source)
        {
            if (ResolveBehavior == null)
            {
                ResolveBehavior = Type("LayoutSegmentBehavior").GetMethod("Resolve", Flags, null,
                    new[] { Type("LayoutSegmentSource") }, null);
                if (ResolveBehavior == null || !ResolveBehavior.IsStatic ||
                    ResolveBehavior.ReturnType != typeof(Nullable<>).MakeGenericType(Type("NativeCompatibility")))
                    throw new PllContractException("Missing or incompatible LayoutSegmentBehavior.Resolve(Source).");
            }
            return ResolveBehavior.Invoke(null, new[] { source });
        }
    }
}
