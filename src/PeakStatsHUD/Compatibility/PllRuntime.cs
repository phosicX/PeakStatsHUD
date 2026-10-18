using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Peak;
using UnityEngine;

namespace PeakStatsEx.Compatibility;

internal sealed partial class PLLCompatibility
{
    private object? routeRun;
    private Array? routeSlots;
    private int routeRevision;
    private int[]? routeIndices;
    private MountainProgressHandler.ProgressPoint[]? routePoints;
    private PllRouteProjection? routeProjection;
    private PllContext? hazardContext;
    private LavaRising? hazard;
    private float nextHazardLookup;
    private PllStatus hazardLookupStatus;

    internal PllStatus ReadRoute(PllContext value, MountainProgressHandler? progress, out PllRouteProjection? projection)
    {
        PllStatus result = Read(PllFeature.Route, () =>
        {
            object state = Get("ProgressProjection", "State",
                Get("MapRun", "Progress", value.Run, "ProgressProjection"), "ProgressProjection+ProgressState")!;
            if (progress == null || progress.gameObject.scene != cachedScene || progress.progressPoints == null ||
                !ReferenceEquals(Get<MountainProgressHandler>("ProgressProjection+ProgressState", "Owner", state), progress))
                return PllStatus.Pending;
            int[] indices = Get<int[]>("ProgressProjection+ProgressState", "PointIndices", state);
            if (indices == null) return Fail(PllFeature.Route, "Published PointIndices is null.");
            if (indices.Length == 0) return PllStatus.Pending;
            MountainProgressHandler.ProgressPoint? peak;
            int peakSlot;
            if (contract!.HasPeakProgress)
            {
                object? peakProgress = Get("ProgressProjection+ProgressState", "Peak", state,
                    "ProgressProjection+PeakProgress");
                peak = peakProgress == null ? null :
                    Get<MountainProgressHandler.ProgressPoint>("ProgressProjection+PeakProgress", "Point", peakProgress);
                peakSlot = peakProgress == null ? -1 :
                    Get<int>("ProgressProjection+PeakProgress", "SlotIndex", peakProgress);
            }
            else
            {
                peak = Get<MountainProgressHandler.ProgressPoint>("ProgressProjection+ProgressState", "PeakPoint", state);
                peakSlot = Get<int>("ProgressProjection+ProgressState", "PeakSlot", state);
            }
            if (ReferenceEquals(routeRun, value.Run) && ReferenceEquals(routeSlots, value.Slots) &&
                routeRevision == value.Revision && ReferenceEquals(routeIndices, indices) &&
                ReferenceEquals(routePoints, progress.progressPoints) && routeProjection != null &&
                ReferenceEquals(routeProjection.Peak, peak) && routeProjection.PeakSlot == peakSlot)
                return PllStatus.Ready;
            if (indices.Length != value.Slots.Length)
                return Fail(PllFeature.Route, "PointIndices does not cover every published slot.");
            IDictionary owners = Get<IDictionary>("ProgressProjection+ProgressState", "Slots", state);
            if (owners == null) return Fail(PllFeature.Route, "Published progress ownership dictionary is null.");
            var segments = new MapHandler.MapSegment[indices.Length];
            var points = new MountainProgressHandler.ProgressPoint[indices.Length];
            var nativeTitleKeys = new string?[indices.Length];
            Type nativeSourceType = contract!.Type("INativeLayoutSegmentSource");
            if (!nativeSourceType.IsInterface)
                return Fail(PllFeature.Route, "INativeLayoutSegmentSource is not an interface.");
            for (int i = 0; i < indices.Length; i++)
            {
                object? slot = value.Slots.GetValue(i);
                if (slot == null || Get<int>("MapSlot", "Index", slot) != i ||
                    indices[i] < 0 || indices[i] >= progress.progressPoints.Length)
                    return Fail(PllFeature.Route, $"Invalid slot/progress index at {i}.");
                var point = progress.progressPoints[indices[i]];
                var segment = Get<MapHandler.MapSegment>("MapSlot", "Segment", slot);
                if (segment == null || point?.transform == null || point.transform.gameObject.scene != cachedScene ||
                    cachedHandler?.segments == null || i >= cachedHandler.segments.Length ||
                    !ReferenceEquals(cachedHandler.segments[i], segment) ||
                    !owners.Contains(point) || owners[point] is not int owner || owner != i)
                    return Fail(PllFeature.Route, $"Progress point ownership does not match slot {i}.");
                segments[i] = segment; points[i] = point;
                object? source = Get("MapSlot", "Source", slot, "LayoutSegmentSource");
                if (source == null) return Fail(PllFeature.Route, $"Published source is null at slot {i}.");
                if (nativeSourceType.IsInstanceOfType(source))
                {
                    string id = Get<string>("LayoutSegmentSource", "Id", source);
                    if (string.IsNullOrWhiteSpace(id))
                        return Fail(PllFeature.Route, $"Native source ID is empty at slot {i}.");
                    nativeTitleKeys[i] = id switch
                    {
                        "Beach" => "SHORE",
                        "Tropics" => "TROPICS",
                        "Roots" => "ROOTS",
                        "Alpine" => "ALPINE",
                        "Mesa" => "MESA",
                        "Caldera" => "CALDERA",
                        "Volcano" => "THE KILN",
                        "Gloom" => "GLOOM",
                        "Citadel" => "THE CITADEL",
                        "Peak" => "PEAK",
                        "Nadir" => "AREA_VOID",
                        _ => null,
                    };
                }
            }
            if (peak != null && (peak.transform == null || peak.transform.gameObject.scene != cachedScene ||
                peak.biome != Biome.BiomeType.Peak || peakSlot < 0 || peakSlot >= points.Length ||
                Array.IndexOf(points, peak) >= 0 || Array.IndexOf(progress.progressPoints, peak) < 0))
                return Fail(PllFeature.Route, "PeakPoint/PeakSlot does not identify an attached Peak.");
            routeRun = value.Run; routeSlots = value.Slots; routeRevision = value.Revision;
            routeIndices = indices; routePoints = progress.progressPoints;
            routeProjection = new PllRouteProjection(segments, points, nativeTitleKeys, peak, peakSlot);
            return PllStatus.Ready;
        });
        projection = result == PllStatus.Ready ? routeProjection : null;
        return result;
    }

    internal PllStatus ReadVariant(PllContext value, out bool custom, out Transform? container, out string[] names)
    {
        bool isCustom = false;
        Transform? occurrenceRoot = null;
        string[] resolved = Array.Empty<string>();
        PllStatus result = Read(PllFeature.Variant, () =>
        {
            if (value.Slot == null) return PllStatus.Ready;
            isCustom = Get("MapSlot", "CustomResolved", value.Slot, "ResolvedSegment") != null;
            occurrenceRoot = Get<Transform>("FeatureOccurrence", "Container", value.Occurrence);
            if (!isCustom) return PllStatus.Ready;
            Transform? root = occurrenceRoot;
            if (root == null || root.gameObject.scene != cachedScene || !root.gameObject.activeInHierarchy)
                return PllStatus.Pending;
            IEnumerable? selections = (IEnumerable?)contract!.Get("FeatureOccurrence", "Variations", value.Occurrence,
                typeof(IReadOnlyList<>).MakeGenericType(contract.Type("VariationSelection")));
            if (selections == null) return Fail(PllFeature.Variant, "Occurrence Variations is null.");
            List<string> variants = new();
            foreach (object selection in selections)
            {
                string path = Get<string>("VariationSelection", "RootPath", selection);
                if (string.IsNullOrEmpty(path)) return Fail(PllFeature.Variant, "Variation RootPath is empty.");
                Transform? chosen = ResolveVariationPath(root, path);
                if (chosen == null || !chosen.IsChildOf(root) || chosen.gameObject.scene != cachedScene)
                    return Fail(PllFeature.Variant, $"Cannot resolve occurrence-relative variation path '{path}'.");
                if (!chosen.gameObject.activeInHierarchy) return PllStatus.Pending;
                variants.Add(chosen.name);
            }
            resolved = variants.ToArray();
            return PllStatus.Ready;
        });
        custom = isCustom; container = occurrenceRoot; names = resolved;
        return result;
    }

    private static Transform? ResolveVariationPath(Transform root, string path)
    {
        int colon = path.IndexOf(':');
        if (colon < 0 || !TryHex(path.Substring(0, colon), out int rootIndex) || rootIndex != 0) return null;
        string[] parts = path.Substring(colon + 1).Split('/');
        Transform current = root;
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            if (part.StartsWith("#", StringComparison.Ordinal))
                return i == parts.Length - 1 && TryHex(part.Substring(1), out int componentIndex) && componentIndex == 0
                    ? current : null;
            if (!TryHex(part, out int sibling) || sibling < 0 || sibling >= current.childCount) return null;
            current = current.GetChild(sibling);
        }
        return null;
    }

    private static bool TryHex(string text, out int value) =>
        int.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value);

    internal PllStatus ReadCampfire(PllContext value, out Campfire? campfire)
    {
        Campfire? resolved = null;
        PllStatus result = Read(PllFeature.Campfire, () =>
        {
            GameObject root = value.Segment.segmentCampfire;
            if (root == null) return PllStatus.Pending;
            if (root.scene != cachedScene) return Fail(PllFeature.Campfire, "The campfire root belongs to another scene.");
            Campfire[] candidates = root.GetComponentsInChildren<Campfire>(true);
            if (candidates.Length == 0) return PllStatus.Pending;
            if (candidates.Length != 1)
                return Fail(PllFeature.Campfire, $"Multiple campfires belong to slot {value.Index}.");
            resolved = candidates[0];
            return PllStatus.Ready;
        });
        campfire = result == PllStatus.Ready ? resolved : null;
        return result;
    }

    internal PllStatus ReadPeakCampfire(PllContext value, out Campfire? campfire)
    {
        Campfire? resolved = null;
        PllStatus result = Read(PllFeature.PeakCampfire, () =>
        {
            if (value.Slot == null) return PllStatus.NotApplicable;
            object? target = Get("MapSlot", "BridgeTarget", value.Slot, "MapSlot+PeakBridgeTarget");
            if (target == null) return PllStatus.NotApplicable;
            object runtime = Get("PeakSuccessorRuntime", "Campfire",
                Get("MapRun", "Peak", value.Run, "PeakSuccessorRuntime"), "PeakCampfireRuntime")!;
            if (!ReferenceEquals(Get("PeakCampfireRuntime", "Owner", runtime, "MapRun"), value.Run))
                return Fail(PllFeature.PeakCampfire, "The campfire runtime belongs to another run.");
            Array fixtures = (Array)Get("PeakCampfireRuntime", "Fixtures", runtime, "PeakCampfireRuntime+PeakCampfireFixture[]")!;
            if (fixtures == null) return Fail(PllFeature.PeakCampfire, "Peak campfire Fixtures is null.");
            if (value.Index >= fixtures.Length || fixtures.GetValue(value.Index) is not { } fixture)
                return PllStatus.Pending;
            Transform root = Get<Transform>("PeakCampfireRuntime+PeakCampfireFixture", "Root", fixture);
            Transform targetRoot = Get<Transform>("MapSlot+PeakBridgeTarget", "Root", target);
            resolved = Get<Campfire>("PeakCampfireRuntime+PeakCampfireFixture", "Campfire", fixture);
            if (root == null || resolved == null) return PllStatus.Pending;
            if (Get<int>("PeakCampfireRuntime+PeakCampfireFixture", "Index", fixture) != value.Index ||
                targetRoot == null || !root.IsChildOf(targetRoot) || !resolved.transform.IsChildOf(root) ||
                resolved.gameObject.scene != cachedScene)
                return Fail(PllFeature.PeakCampfire, $"Campfire fixture does not belong to slot {value.Index}.");
            return PllStatus.Ready;
        });
        campfire = result == PllStatus.Ready ? resolved : null;
        return result;
    }

    internal PllStatus ReadNadir(PllContext value, out bool isNadir, out PeakGatePortal? portal)
    {
        bool nadir = value.IsVoidController;
        PeakGatePortal? resolved = null;
        PllStatus result = Read(PllFeature.Portal, () =>
        {
            Transform? gate = value.Slot == null ? null : Get<Transform>("MapSlot", "NadirGate", value.Slot);
            nadir |= gate != null;
            if (!nadir) return PllStatus.NotApplicable;
            if (gate != null) resolved = gate.GetComponent<PeakGatePortal>();
            else if (value.Segment.segmentParent != null)
            {
                var portals = value.Segment.segmentParent.GetComponentsInChildren<PeakGatePortal>(true);
                if (portals.Length > 1) return Fail(PllFeature.Portal, "Multiple portals belong to the current Void controller.");
                if (portals.Length == 1) resolved = portals[0];
            }
            if (resolved == null) return PllStatus.Pending;
            if (gate != null)
            {
                Transform container = Get<Transform>("FeatureOccurrence", "Container", value.Occurrence);
                if (container == null || !gate.IsChildOf(container))
                    return Fail(PllFeature.Portal, "NadirGate is outside the current occurrence.");
            }
            if (resolved.gameObject.scene != cachedScene)
                return Fail(PllFeature.Portal, "The current Nadir portal belongs to another scene.");
            return PllStatus.Ready;
        });
        isNadir = nadir; portal = result == PllStatus.Ready ? resolved : null;
        return result;
    }

    internal PllStatus ReadHazard(PllContext value, out LavaRising? risingField)
    {
        risingField = null;
        if (failedFeatures.Contains(PllFeature.Hazard)) return PllStatus.Unsupported;
        bool changed = hazardContext == null || !ReferenceEquals(hazardContext.Run, value.Run) ||
            hazardContext.Index != value.Index || !ReferenceEquals(hazardContext.Occurrence, value.Occurrence) ||
            hazardContext.Revision != value.Revision;
        bool destroyed = hazard == null && !ReferenceEquals(hazard, null);
        if (!changed && !destroyed && (hazard != null || Time.unscaledTime < nextHazardLookup))
        {
            risingField = hazard;
            return hazardLookupStatus;
        }
        PllStatus result = ResolveHazard(value);
        risingField = result == PllStatus.Ready ? hazard : null;
        return result;
    }

    private PllStatus ResolveHazard(PllContext value)
    {
        return Read(PllFeature.Hazard, () =>
        {
            hazardContext = value;
            nextHazardLookup = Time.unscaledTime + 1f;
            hazardLookupStatus = PllStatus.Pending;
            hazard = null;
            Transform? container = value.Slot == null ? value.Segment.segmentParent?.transform :
                Get<Transform>("FeatureOccurrence", "Container", value.Occurrence);
            if (container == null || container.gameObject.scene != cachedScene) return PllStatus.Pending;
            Segment? role = value.Slot == null ? Segment.Void : null;
            if (value.Slot != null)
            {
                object source = Get("MapSlot", "Source", value.Slot, "LayoutSegmentSource")!;
                object? behavior = contract!.Resolve(source);
                if (behavior != null) role = Get<Segment>("NativeCompatibility", "Role", behavior);
            }
            IReadOnlyList<Transform> roots = value.Slot == null ? new[] { container } :
                Get<IReadOnlyList<Transform>>("FeatureOccurrence", "Roots", value.Occurrence);
            if (roots == null) return Fail(PllFeature.Hazard, "Occurrence Roots is null.");
            foreach (LavaRising candidate in container.GetComponentsInChildren<LavaRising>(true))
            {
                bool owned = false;
                foreach (object rawRoot in roots)
                {
                    if (rawRoot is not Transform root || root == null || root.gameObject.scene != cachedScene ||
                        !root.IsChildOf(container)) return Fail(PllFeature.Hazard, "Invalid occurrence ownership root.");
                    owned |= candidate.transform.IsChildOf(root);
                }
                if (!owned) continue;
                if (!role.HasValue) return Fail(PllFeature.Hazard, "A rising field has no reliable native behavior role.");
                if (candidate.requiredSegment != role.Value) continue;
                if (hazard != null) return Fail(PllFeature.Hazard, "Multiple rising fields match the current occurrence and native role.");
                hazard = candidate;
            }
            hazardLookupStatus = PllStatus.Ready;
            return hazardLookupStatus;
        });
    }
}
