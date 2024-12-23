using Newtonsoft.Json;
using UnityEngine;

namespace HoyarCreation.TriadCluster
{
    [System.Serializable]
    public class TriadClusterUnitPreset
    {
        public int id;
        public float radius;
        public float[] angleWeights;
        [JsonIgnore] public float[] actualAngles;
        [JsonIgnore] public Vector2[] pointLocalPositions;

        public void InitMetadata()
        {
            float totalWeight = 0f;
            foreach (float weight in angleWeights)
            {
                totalWeight += weight;
            }

            actualAngles = new float[3];
            for (int i = 0; i < actualAngles.Length; i++)
            {
                actualAngles[i] = angleWeights[i] / totalWeight * 360f;
            }

            pointLocalPositions = new Vector2[3];
            for (int i = 0; i < pointLocalPositions.Length; i++)
            {
                float angle = 0f;
                for (int j = 0; j <= i; j++)
                {
                    angle += actualAngles[j];
                }

                angle = angle * Mathf.Deg2Rad;

                Vector2 pointLocalPosition = new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));

                pointLocalPositions[i] = pointLocalPosition;
            }
        }
    }
}