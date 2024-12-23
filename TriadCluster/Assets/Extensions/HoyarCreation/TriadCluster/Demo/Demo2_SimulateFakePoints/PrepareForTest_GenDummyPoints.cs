using System;
using System.Collections.Generic;
using MessagePipe;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HoyarCreation.TriadCluster.Demo
{
    public class PrepareForTest_GenDummyPoints : MonoBehaviour
    {
        public bool sendInputEventsByTriangleUnitPoints;
    
        private List<Tuple<int, int, int>> m_AngleWeightCombinations;
        private float m_SpawnScopeDimension_H => Camera.main.pixelWidth;
        private float m_SpawnScopeDimension_V => Camera.main.pixelHeight;

        [SerializeField] private List<Transform> m_TriangleUnits = new();
        private readonly List<Transform> m_TriangleUnitPoints = new();

        [SerializeField]  private float m_UnitRadius = 100f;
    
        [Space]
        [Header("GizmoSettings")] 
        [SerializeField] private bool showUnitPoints;
        [SerializeField] private bool showSubPoints;
        [SerializeField] private float subPointGizmoRadius = 20f;

        private static IPublisher<UpdateInputPointsEvent> m_EventSender_UpdateInputPoints => GlobalMessagePipe.GetPublisher<UpdateInputPointsEvent>();

        private void Update()
        {
            if (sendInputEventsByTriangleUnitPoints)
            {
                var allPoints = new List<Vector2>(m_TriangleUnitPoints.Count);
                foreach (var point in m_TriangleUnitPoints)
                {
                    allPoints.Add(point.position);
                }

                m_EventSender_UpdateInputPoints.Publish(new UpdateInputPointsEvent(allPoints));
            }
        }

        [ContextMenu("MakeAllPossibleTrianglesTemplate_05Degree")] // 91 combinations
        public void MakeAllPossibleTrianglesTemplate_05Degree() => MakeAllPossibleTrianglesCombinations(5);

        [ContextMenu("MakeAllPossibleTrianglesTemplate_10Degree")] // 19 combinations
        public void MakeAllPossibleTrianglesTemplate_10Degree() => MakeAllPossibleTrianglesCombinations(10);

        [ContextMenu("MakeAllPossibleTrianglesTemplate_12Degree")] // 12 combinations
        public void MakeAllPossibleTrianglesTemplate_12Degree() => MakeAllPossibleTrianglesCombinations(12);

        [ContextMenu("MakeAllPossibleTrianglesTemplate_15Degree")] // 7 combinations
        public void MakeAllPossibleTrianglesTemplate_15Degree() => MakeAllPossibleTrianglesCombinations(15);

        private void MakeAllPossibleTrianglesCombinations(int angleStep)
        {
            m_AngleWeightCombinations = FindTriangles();

            List<Tuple<int, int, int>> FindTriangles()
            {
                var triangles = new List<Tuple<int, int, int>>();
                for (int angle1 = 180; angle1 >= angleStep; angle1 -= angleStep)
                {
                    for (int angle2 = angle1; angle2 >= angleStep; angle2 -= angleStep)
                    {
                        for (int angle3 = angle2; angle3 >= angleStep; angle3 -= angleStep)
                        {
                            if (angle1 + angle2 + angle3 == 180 && angle1 > angle2 && angle2 > angle3)
                            {
                                triangles.Add(Tuple.Create(angle1, angle2, angle3));
                            }
                        }
                    }
                }

                return triangles;
            }
        }

        [ContextMenu("Setup_FixedSeedRandomPosition")]
        public void Setup_FixedSeedRandomPosition() => Setup(true, true);

        [ContextMenu("Setup_RandomPosition")]
        public void Setup_RandomPosition() => Setup(true);

        [ContextMenu("Setup_BenchmarkPosition")]
        public void Setup_BenchmarkPosition() => Setup(false);
    
        public void Setup(bool isRandomLayout, bool isFixedSeed = false)
        {
            for (int i = m_TriangleUnits.Count - 1; i >= 0; i--)
            {
                Destroy(m_TriangleUnits[i].gameObject);
            }

            m_TriangleUnits.Clear();
            m_TriangleUnitPoints.Clear();
        
            for (int i = 0; i < m_AngleWeightCombinations.Count; i++)
            {
                var triangleUnit = new GameObject("TriangleUnit");
                var point1 = new GameObject("Point1");
                var point2 = new GameObject("Point2");
                var point3 = new GameObject("Point3");
                triangleUnit.transform.SetParent(transform);
                point1.transform.SetParent(triangleUnit.transform);
                point2.transform.SetParent(triangleUnit.transform);
                point3.transform.SetParent(triangleUnit.transform);
                m_TriangleUnits.Add(triangleUnit.transform);
                m_TriangleUnitPoints.Add(point1.transform);
                m_TriangleUnitPoints.Add(point2.transform);
                m_TriangleUnitPoints.Add(point3.transform);
        
                if (isRandomLayout)
                {
                    if (isFixedSeed) Random.InitState(i);
                    triangleUnit.transform.position = new Vector3(
                        Random.Range(0, m_SpawnScopeDimension_H),
                        Random.Range(0, m_SpawnScopeDimension_V),
                        0);
                }
                else
                {
                    float sideLength = m_SpawnScopeDimension_H * 2f;
                    const int countPerRow = 10;
                    float gap = sideLength / countPerRow;
                    int rowIdx = Mathf.FloorToInt((float)i / countPerRow);
                    triangleUnit.transform.position = new Vector3(
                        0 + i % countPerRow * gap,
                        m_SpawnScopeDimension_V - rowIdx * gap,
                        0);
                }

                Vector2 center = triangleUnit.transform.position;
                var subPoints = new Transform[3] { point1.transform, point2.transform, point3.transform };
                var angleWeights = new int[3] {m_AngleWeightCombinations[i].Item1, m_AngleWeightCombinations[i].Item2, m_AngleWeightCombinations[i].Item3};
                UpdatePoints(center, subPoints, angleWeights);
            }

            void UpdatePoints(Vector2 center, Transform[] points,int[] angleWeights)
            {
                float totalWeight = angleWeights[0]+ angleWeights[1] + angleWeights[2];

                float[] actualAngles = new float[angleWeights.Length];
                for (int i = 0; i < angleWeights.Length; i++)
                {
                    actualAngles[i] = angleWeights[i] / totalWeight * 360f;
                }

                for (int i = 0; i < points.Length; i++)
                {
                    float angle = 0f;
                    for (int j = 0; j <= i; j++)
                    {
                        angle += actualAngles[j];
                    }

                    // angle = (angle + orientationOffsetAngle * 360f) * Mathf.Deg2Rad;
                    angle *=  Mathf.Deg2Rad;
                    var pointPosition = new Vector2(center.x + m_UnitRadius * Mathf.Cos(angle), center.y + m_UnitRadius * Mathf.Sin(angle));
                
                    points[i].position = pointPosition;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (showUnitPoints)
            {
                Gizmos.color = Color.white;
                foreach (var unit in m_TriangleUnits)
                {
                    Gizmos.DrawWireSphere(unit.position, m_UnitRadius);
                }
            }
        
            if (showSubPoints)
            {
                Gizmos.color = Color.blue;
                foreach (var point in m_TriangleUnitPoints)
                {
                    Gizmos.DrawWireSphere(point.position, subPointGizmoRadius);
                }
            }
        }
    }
}
