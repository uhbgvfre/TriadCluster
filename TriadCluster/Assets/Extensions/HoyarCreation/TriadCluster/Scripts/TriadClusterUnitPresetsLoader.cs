using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace HoyarCreation.TriadCluster
{
    public static class TriadClusterUnitPresetsLoader
    {
        public static List<TriadClusterUnitPreset> LoadAsUnitPresetsFromPlayerPref()
        {
            var jStr = PlayerPrefs.GetString(k_ConfigKey_TriadClusterUnitPresets, k_DefaultTriadClusterUnitPresetsJsonString);
            var presets = JsonConvert.DeserializeObject<List<TriadClusterUnitPreset>>(jStr);
            presets.ForEach(pst => pst.InitMetadata());

            return presets;
        }

        public static string k_ConfigKey_TriadClusterUnitPresets = "HoyarCreation.TriadCluster.TriadClusterUnitPresets";

        public static readonly string k_DefaultTriadClusterUnitPresetsJsonString = @" 
        [{
            ""id"": 1,
            ""radius"": 100,
            ""angleWeights"": [
                90,
                60,
                30
            ]
        },
        {
            ""id"": 2,
            ""radius"": 100,
            ""angleWeights"": [
                62,
                60,
                58
            ]
        },
        {
            ""id"": 3,
            ""radius"": 100,
            ""angleWeights"": [
                110,
                50,
                20
            ]
        }]";
    }
}