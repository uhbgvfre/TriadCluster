using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HoyarCreation.TriadCluster
{
    public static class TriadClusterConfig
    {
        public static int AnalyzeCycleFrameInterval { get; private set; } = 1;
        public static float MinTriangleCircumradius { get; private set; } = .01f;
        public static float MaxTriangleCircumradius { get; private set; } = 500f;
        public static float GetEps() => MaxTriangleCircumradius * 2f + 0.0001f;
        public static float MaxAbsDeltaAngleDegree { get; private set; } = 2f;
        public static float MaxAbsDeltaAreaRatio { get; private set; } = .0001f;
        public static int FrameCountForNoRespondsThreshold { get; private set; } = 2;
        public static bool IgnoreTriangleAreaMatching { get; private set; } = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            var configJsonString = PlayerPrefs.GetString(k_ConfigKey_GeneralConfig, k_DefaultTriadClusterGeneralConfigJsonString);
            Debug.Log($"[TriadClusterConfig][Init] ReadConfigString: {configJsonString}");
            DeserializeParams(configJsonString);
        }

        private static void DeserializeParams(string config)
        {
            var json = JObject.Parse(config);

            // General
            AnalyzeCycleFrameInterval = (int)json["AnalyzeCycleFrameInterval"];
            MinTriangleCircumradius = (float)json["MinTriangleCircumradius"];
            MaxTriangleCircumradius = (float)json["MaxTriangleCircumradius"];
            MaxAbsDeltaAngleDegree = (float)json["MaxAbsDeltaAngleDegree"];
            MaxAbsDeltaAreaRatio = (float)json["MaxAbsDeltaAreaRatio"];
            FrameCountForNoRespondsThreshold = (int)json["FrameCountForNoRespondsThreshold"];
            IgnoreTriangleAreaMatching = (bool)json["IgnoreTriangleAreaMatching"];

            /* For Debugging */
            // Debug.Log("===TriadClusterConfig===");
            // Debug.Log($"AnalyzeCycleFrameInterval: {AnalyzeCycleFrameInterval}");
            // Debug.Log($"MinTriangleCircumradius: {MinTriangleCircumradius}");
            // Debug.Log($"MaxTriangleCircumradius: {MaxTriangleCircumradius}");
            // Debug.Log($"MaxAbsDeltaAngleDegree: {MaxAbsDeltaAngleDegree}");
            // Debug.Log($"MaxAbsDeltaAreaRatio: {MaxAbsDeltaAreaRatio}");
            // Debug.Log($"FrameCountForNoRespondsThreshold: {FrameCountForNoRespondsThreshold}");
            // Debug.Log($"IgnoreTriangleAreaMatching: {IgnoreTriangleAreaMatching}");
        }

        public const string k_ConfigKey_GeneralConfig = "HoyarCreation.TriadCluster.GeneralConfig";
        public const string k_DefaultTriadClusterGeneralConfigJsonString = @"
        {
            ""AnalyzeCycleFrameInterval"": 1, // [Default(1)] Tringle match algorithm exute interval expected value 
            ""MinTriangleCircumradius"": 0.01, 
            ""MaxTriangleCircumradius"": 777, 
            ""MaxAbsDeltaAngleDegree"": 2, // [Recommend(2)] In two triangle match case, if any angle difference is greater than this value, it will be ignored
            ""MaxAbsDeltaAreaRatio"": 0.0001, // [Recommend(.0001)] In two triangle match case, if area ratio difference is greater than this value, it will be ignored 
            ""FrameCountForNoRespondsThreshold"": 2, // [Default(2)] if value is greater than this value, define OnTriangleUp 
            ""IgnoreTriangleAreaMatching"": 1 // if TRUE(!0), MaxAbsDeltaAreaRatio will invalid, only compare angles, tolerance will increase
        }";
        
    }
}
