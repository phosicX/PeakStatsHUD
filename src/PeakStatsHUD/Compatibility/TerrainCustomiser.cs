using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PeakStatsEx.Compatibility;

internal static class TerrainCustomiserCompatibility
{
    private const string PluginGuid = "com.snosz.terraincustomiser";
    private const string PlayerInfoManagerTypeName = "TerrainCustomiser.Managers.PlayerInfoManager";

    private const BindingFlags StaticFieldFlags =
        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private const BindingFlags InstanceFieldFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    internal static bool Enabled =>
        BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(PluginGuid);

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool TryGetMapSeed(out int seed)
    {
        seed = default;
        var playerInfoManagerType = typeof(TerrainCustomiser.Plugin).Assembly.GetType(PlayerInfoManagerTypeName);
        var currentMapSyncData = playerInfoManagerType?
            .GetField("currentMapSyncData", StaticFieldFlags)?
            .GetValue(null);

        if (currentMapSyncData?.GetType().GetField("seed", InstanceFieldFlags)?.GetValue(currentMapSyncData) is int mapSeed)
        {
            seed = mapSeed;
            return true;
        }

        return false;
    }
}
