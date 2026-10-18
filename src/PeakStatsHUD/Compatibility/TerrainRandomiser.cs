using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Photon.Pun;
using Steamworks;

namespace PeakStatsEx.Compatibility;

internal static class TerrainRandomiserCompatibility
{
    internal static bool Enabled => BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.snosz.terrainrandomiser");

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool TryGetMapSeed(out int seed)
    {
        seed = default;
        MapSettings? mapSettings = GetMapSettings();
        if (mapSettings?.enableRandomiser != true)
        {
            return false;
        }

        seed = mapSettings.seed;
        return true;
    }

    private static MapSettings? GetMapSettings()
    {
        if (!Enabled)
            return null;

        if (PhotonNetwork.OfflineMode)
            return GetLocalMapSettings();

        SteamLobbyHandler service = GameHandler.GetService<SteamLobbyHandler>();
        string lobbyData = SteamMatchmaking.GetLobbyData(service.LobbySteamId, "TR_roomMapSettings");
        return string.IsNullOrWhiteSpace(lobbyData)
            ? null
            : JsonConvert.DeserializeObject<MapSettings>(lobbyData);
    }

    private static MapSettings? GetLocalMapSettings()
    {
        TerrainRandomiser.MapSettings? mapSettings = TerrainRandomiser.Plugin.Instance?.mapSettings;
        if (mapSettings == null)
            return null;

        string json = JsonConvert.SerializeObject(mapSettings);
        return JsonConvert.DeserializeObject<MapSettings>(json);
    }

}

[Serializable]
internal class MapSettings
{
    public bool enableRandomiser = false;

    public int seed = 0;

    public bool autoRandomSeed = false;

    public bool fullyRandom = false;

    public List<BiomeSettings> biomes = [];
}

[Serializable]
internal class BiomeSettings
{
    public bool overrideEnabled = false;

    public string sectionName = "";

    public BiomeData selectedBiome = new();

    public string selectedVariant = "";

    public bool randomBiome = false;

    public bool randomVariant = false;
}

[Serializable]
internal class BiomeData
{
    public string biomeName = "";

    public Biome.BiomeType biomeType = default;

    public List<string> variants = [];

    public VariantSelectionType variantSelectionType = VariantSelectionType.BiomeVariant;
}

public enum VariantSelectionType
{
    BiomeVariant,
    VariantObject
}
