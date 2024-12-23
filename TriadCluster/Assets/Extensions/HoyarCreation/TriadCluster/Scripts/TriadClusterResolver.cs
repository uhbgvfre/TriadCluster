using MessagePipe;
using System.Collections.Generic;
using System.Linq;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace HoyarCreation.TriadCluster
{
    public class TriadClusterResolver : MonoBehaviour
    {
        [SerializeField] private bool loadUnitPresetsOnStart = true; // If you want to decide the presets load time, you can set it to FALSE and use TriadClusterUnitPresetsLoader to initialize it at the right time

        [Header("Info")]
        public int clustersCount;
        public int weavedValidTrianglesCount;
        public int matchedTrianglesCount;

        [Header("Config")]
        [SerializeField] private float m_AnalyzeCycleFrameInterval = TriadClusterConfig.AnalyzeCycleFrameInterval;

        [SerializeField] private float m_Epsilon = TriadClusterConfig.GetEps();

        [SerializeField] private float m_MinSideLength = TriadClusterConfig.MinTriangleCircumradius;

        [SerializeField] private float m_MaxAbsDeltaAngleDegree = TriadClusterConfig.MaxAbsDeltaAngleDegree;

        [SerializeField] private float m_MaxAbsDeltaAreaRatio = TriadClusterConfig.MaxAbsDeltaAreaRatio;

        [SerializeField] private bool m_IgnoreTriangleAreaMatching = TriadClusterConfig.IgnoreTriangleAreaMatching;

        private JobTriangle[] m_UnitPresetTriangles;
        private List<Vector2> m_InputPoints;

        private List<List<Vector2>> m_Clusters; // Results of step1: Clustering
        private JobTriangle[] m_WeavedValidTriangles; //  Results of step2: ClustersToTriangles
        private JobTriangle[] m_MatchedTriangles; // Results of step3: TrianglesMatching

        private ISubscriber<UpdateInputPointsEvent> m_OnUpdateInputPoints => GlobalMessagePipe.GetSubscriber<UpdateInputPointsEvent>();

        private IDisposable m_OnUpdateInputPointsEventHandle;
        
        private NativeArray<int> m_TriangularNumbersNativeArray = new(TriangularNumber.GetCachedTerm0ToTerm999Numbers(), Allocator.Persistent);

        private NativeArray<int> m_TetrahedralNumbersNativeArray = new(TetrahedralNumber.GetCachedTerm0ToTerm999Numbers(), Allocator.Persistent);

        private const int k_MinPts = 3;

        private void OnDestroy()
        {
            m_TriangularNumbersNativeArray.Dispose();
            m_TetrahedralNumbersNativeArray.Dispose();
            m_OnUpdateInputPointsEventHandle.Dispose();
        }

        private void Start()
        {
            BindEvents();

            if (loadUnitPresetsOnStart)
            {
                LoadUnitPresetsAsJobTriangles();
            }
        }

        private void BindEvents()
        {
            m_OnUpdateInputPointsEventHandle = m_OnUpdateInputPoints
                .Subscribe(e => { m_InputPoints = e.inputPoints ?? new List<Vector2>(); });
        }

        public JobTriangle[] GetMatchedJobTriangles() => m_MatchedTriangles ?? Array.Empty<JobTriangle>();

        private void LoadUnitPresetsAsJobTriangles()
        {
            List<TriadClusterUnitPreset> presets = TriadClusterUnitPresetsLoader.LoadAsUnitPresetsFromPlayerPref();
            m_UnitPresetTriangles = presets.Select(pst => new JobTriangle()
            {
                pointA = new float2(pst.pointLocalPositions[0].x, pst.pointLocalPositions[0].y),
                pointB = new float2(pst.pointLocalPositions[1].x, pst.pointLocalPositions[1].y),
                pointC = new float2(pst.pointLocalPositions[2].x, pst.pointLocalPositions[2].y),
                idLabel = pst.id
            }).ToArray();
        }

        private void Update()
        {
            if (m_InputPoints == null) return;

            if (Time.frameCount % m_AnalyzeCycleFrameInterval == 0)
            {
                CompleteMatchJob();
                ResolveAndScheduleToMatch();
            }
        }

        private void ResolveAndScheduleToMatch()
        {
            Clustering();
            ChangeClustersToTriangles();
            ScheduleMatchJob();
        }

        private void Clustering()
        {
            List<(float, float)> points = m_InputPoints.Select(p => (p.x, p.y)).ToList();
            var clusterIDs = DBSCAN.Run(points, m_Epsilon, k_MinPts);

            List<List<Vector2>> groups = new();

            clustersCount = 0;
            if (clusterIDs.Count > 0)
            {
                int maxLabel = clusterIDs.Max();
                for (int i = 1; i <= maxLabel; i++)
                {
                    List<Vector2> group = new();
                    for (int j = 0; j < clusterIDs.Count; j++)
                    {
                        if (clusterIDs[j] == i)
                        {
                            group.Add(new Vector2(points[j].Item1, points[j].Item2));
                        }
                    }

                    groups.Add(group);
                    clustersCount++;
                }
            }

            m_Clusters = groups;
        }

        private void ChangeClustersToTriangles()
        {
            var subWeavedValidTriangles = new NativeArray<JobTriangle>[m_Clusters.Count];
            for (int i = 0; i < m_Clusters.Count; i++)
            {
                var cluster = m_Clusters[i];
                NativeArray<Vector2> points = new(cluster.ToArray(), Allocator.TempJob);
                NativeQueue<JobTriangle> weavedValidTrianglesNativeQueue = new(Allocator.TempJob);

                var job = new WeaveAndFilterTrianglesJob()
                {
                    points = points,
                    maxSideLength = m_Epsilon,
                    minSideLength = m_MinSideLength,
                    triangularNumbersTable = m_TriangularNumbersNativeArray,
                    tetrahedralNumbersTable = m_TetrahedralNumbersNativeArray,
                    weavedValidTriangles = weavedValidTrianglesNativeQueue.AsParallelWriter(),
                };

                job.Schedule(points.Length - 2, 8).Complete();

                subWeavedValidTriangles[i] = weavedValidTrianglesNativeQueue.ToArray(Allocator.TempJob);

                points.Dispose();
                weavedValidTrianglesNativeQueue.Dispose();
            }

            m_WeavedValidTriangles = subWeavedValidTriangles.SelectMany(x => x).ToArray();
            weavedValidTrianglesCount = m_WeavedValidTriangles.Length;

            foreach (var subTriArray in subWeavedValidTriangles)
            {
                subTriArray.Dispose();
            }
        }

        [BurstCompile]
        private struct WeaveAndFilterTrianglesJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector2> points;
            [ReadOnly] public float maxSideLength;
            [ReadOnly] public float minSideLength;
            [ReadOnly] public NativeArray<int> triangularNumbersTable;
            [ReadOnly] public NativeArray<int> tetrahedralNumbersTable;

            [NativeDisableParallelForRestriction] public NativeQueue<JobTriangle>.ParallelWriter weavedValidTriangles;

            public void Execute(int i)
            {
                int term = points.Length - 2;
                for (int j = i + 1; j < term + 1; j++)
                {
                    for (int k = j + 1; k < term + 2; k++)
                    {
                        bool isAnySideOverlength =
                            IsDistanceOverSuitableLength(points[i], points[j]) ||
                            IsDistanceOverSuitableLength(points[j], points[k]) ||
                            IsDistanceOverSuitableLength(points[k], points[i]);

                        if (isAnySideOverlength) continue;

                        weavedValidTriangles.Enqueue(new JobTriangle
                        {
                            pointA = points[i],
                            pointB = points[j],
                            pointC = points[k],
                        });
                    }
                }
            }

            bool IsDistanceOverSuitableLength(float2 pointA, float2 pointB)
            {
                float dist = math.distance(pointA, pointB);
                return dist > maxSideLength || dist < minSideLength;
            }
        }

        private NativeArray<JobTriangle> m_UnitPresetTrianglesNativeArrayBuffer;
        private NativeArray<JobTriangle> m_WeavedValidTrianglesNativeArrayBuffer;
        private NativeQueue<JobTriangle> m_MatchedTrianglesNativeArrayBuffer;
        private JobHandle m_MatchJobHandle;

        private void ScheduleMatchJob()
        {
            m_UnitPresetTrianglesNativeArrayBuffer = new(m_UnitPresetTriangles, Allocator.TempJob);
            var precalcTrianglesAnglesAndAreaJob_UnitPresetTriangles = new PrecalcTrianglesAnglesAndAreaJob()
            {
                triangles = m_UnitPresetTrianglesNativeArrayBuffer
            };

            m_WeavedValidTrianglesNativeArrayBuffer = new NativeArray<JobTriangle>(m_WeavedValidTriangles, Allocator.TempJob);
            var precalcTrianglesAnglesAndAreaJob_WeavedValidTriangles = new PrecalcTrianglesAnglesAndAreaJob()
            {
                triangles = m_WeavedValidTrianglesNativeArrayBuffer
            };

            m_MatchedTrianglesNativeArrayBuffer = new(Allocator.TempJob);
            var matchTrianglesAndPostcalcJob = new MatchTrianglesAndPostcalcJob()
            {
                unitPresetTriangles = m_UnitPresetTrianglesNativeArrayBuffer,
                weavedValidTriangles = m_WeavedValidTrianglesNativeArrayBuffer,
                matchedTriangles = m_MatchedTrianglesNativeArrayBuffer.AsParallelWriter(),
                maxAbsDeltaAngleDegree = m_MaxAbsDeltaAngleDegree,
                maxAbsDeltaAreaRatio = m_MaxAbsDeltaAreaRatio,
                ignoreTriangleAreaMatching = m_IgnoreTriangleAreaMatching,
            };

            var precalcJobHandle = JobHandle.CombineDependencies(
                precalcTrianglesAnglesAndAreaJob_UnitPresetTriangles.Schedule(m_UnitPresetTrianglesNativeArrayBuffer.Length, 8),
                precalcTrianglesAnglesAndAreaJob_WeavedValidTriangles.Schedule(m_WeavedValidTrianglesNativeArrayBuffer.Length, 8)
            );

            m_MatchJobHandle = matchTrianglesAndPostcalcJob.Schedule(m_WeavedValidTrianglesNativeArrayBuffer.Length, 8, precalcJobHandle);
        }

        private void CompleteMatchJob()
        {
            m_MatchJobHandle.Complete();

            bool isNoDatas = m_MatchedTrianglesNativeArrayBuffer.IsEmpty();
            m_MatchedTriangles = isNoDatas ? null : m_MatchedTrianglesNativeArrayBuffer.ToArray(Allocator.TempJob).ToArray();
            matchedTrianglesCount = isNoDatas ? 0 : m_MatchedTriangles.Length;

            if (m_UnitPresetTrianglesNativeArrayBuffer.IsCreated) m_UnitPresetTrianglesNativeArrayBuffer.Dispose();
            if (m_WeavedValidTrianglesNativeArrayBuffer.IsCreated) m_WeavedValidTrianglesNativeArrayBuffer.Dispose();
            if (m_MatchedTrianglesNativeArrayBuffer.IsCreated) m_MatchedTrianglesNativeArrayBuffer.Dispose();
        }

        [BurstCompile]
        private struct PrecalcTrianglesAnglesAndAreaJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] public NativeArray<JobTriangle> triangles;
            const float k_Rad2Deg = 57.29578f;

            public void Execute(int i)
            {
                var tri = triangles[i];

                var pts = new NativeArray<float2>(3, Allocator.Temp);
                pts[0] = tri.pointA;
                pts[1] = tri.pointB;
                pts[2] = tri.pointC;

                var sides = new NativeArray<float>(3, Allocator.Temp);
                var angleDegrees = new NativeArray<float>(3, Allocator.Temp);

                float maxAngleRecord = -1f;
                for (int pIndex = 0; pIndex < 3; pIndex++)
                {
                    int nextIndex = (pIndex + 1) % 3;
                    sides[pIndex] = math.distance(pts[pIndex], pts[nextIndex]);
                    int prevIndex = (pIndex + 2) % 3;
                    float rad = CalculateAngle(pts[prevIndex], pts[pIndex], pts[nextIndex]);
                    float absDeg = math.abs(rad * k_Rad2Deg);
                    angleDegrees[pIndex] = absDeg;

                    if (absDeg >= maxAngleRecord)
                    {
                        maxAngleRecord = absDeg;
                        tri.maxAnglePoint = pts[pIndex];
                    }
                }

                Sort3Pts(angleDegrees);
                tri.angleDegreeMax = angleDegrees[0];
                tri.angleDegreeMid = angleDegrees[1];
                tri.angleDegreeMin = angleDegrees[2];

                tri.area = CalculateArea();

                triangles[i] = tri;

                float CalculateAngle(float2 a, float2 b, float2 c)
                {
                    float2 ab = b - a;
                    float2 cb = b - c;
                    float dot = math.dot(ab, cb);
                    float cross = ab.x * cb.y - ab.y * cb.x;
                    return math.atan2(cross, dot);
                }

                float CalculateArea()
                {
                    float s = (sides[0] + sides[1] + sides[2]) / 2;
                    return math.sqrt(s * (s - sides[0]) * (s - sides[1]) * (s - sides[2]));
                }
            }

            private void Sort3Pts(NativeArray<float> array)
            {
                if (array[0] < array[2]) Swap(0, 2);
                if (array[0] < array[1]) Swap(0, 1);
                if (array[1] < array[2]) Swap(1, 2);

                void Swap(int indexA, int indexB)
                {
                    (array[indexB], array[indexA]) = (array[indexA], array[indexB]);
                }
            }
        }

        [BurstCompile]
        private struct MatchTrianglesAndPostcalcJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<JobTriangle> unitPresetTriangles;
            [ReadOnly] public NativeArray<JobTriangle> weavedValidTriangles;
            [ReadOnly] public float maxAbsDeltaAngleDegree;
            [ReadOnly] public float maxAbsDeltaAreaRatio;
            [ReadOnly] public bool ignoreTriangleAreaMatching;
            [NativeDisableParallelForRestriction] public NativeQueue<JobTriangle>.ParallelWriter matchedTriangles;

            private const float k_Rad2Deg = 57.29578f;

            public void Execute(int index)
            {
                foreach (var unitPresetTriangle in unitPresetTriangles)
                {
                    if (!IsApproxEq(weavedValidTriangles[index], unitPresetTriangle)) continue;
                    var tri = weavedValidTriangles[index];
                    tri.idLabel = unitPresetTriangle.idLabel; // Mark matched id
                    CalculateCircumcircle(ref tri);
                    CalculateOrientation(ref tri);
                    matchedTriangles.Enqueue(tri);
                }
            }

            private bool IsApproxEq(JobTriangle thisTri, JobTriangle otrherTri)
            {
                var absDeltaAngles = new NativeArray<float>(3, Allocator.Temp);
                for (int i = 0; i < 3; i++)
                {
                    absDeltaAngles[0] = math.abs(thisTri.angleDegreeMax - otrherTri.angleDegreeMax);
                    absDeltaAngles[1] = math.abs(thisTri.angleDegreeMid - otrherTri.angleDegreeMid);
                    absDeltaAngles[2] = math.abs(thisTri.angleDegreeMin - otrherTri.angleDegreeMin);
                }

                float absDeltaAreaRatio = ignoreTriangleAreaMatching
                    ? 0
                    : math.abs((thisTri.area - otrherTri.area) / thisTri.area);

                bool isAnyDeltaAngleOverrange =
                    absDeltaAngles[0] > maxAbsDeltaAngleDegree ||
                    absDeltaAngles[1] > maxAbsDeltaAngleDegree ||
                    absDeltaAngles[2] > maxAbsDeltaAngleDegree;

                bool isDeltaAreaRatioOverrange = absDeltaAreaRatio > maxAbsDeltaAreaRatio;

                if (isAnyDeltaAngleOverrange || isDeltaAreaRatioOverrange) return false;

                return true;
            }

            private void CalculateCircumcircle(ref JobTriangle tri)
            {
                // circumcentre
                float2 A = tri.pointA, B = tri.pointB, C = tri.pointC;
                float D = 2 * (A.x * (B.y - C.y) + B.x * (C.y - A.y) + C.x * (A.y - B.y));
                float circumcentreX = ((A.x * A.x + A.y * A.y) * (B.y - C.y) +
                                       (B.x * B.x + B.y * B.y) * (C.y - A.y) +
                                       (C.x * C.x + C.y * C.y) * (A.y - B.y)) / D;
                float circumcentreY = ((A.x * A.x + A.y * A.y) * (C.x - B.x) +
                                       (B.x * B.x + B.y * B.y) * (A.x - C.x) +
                                       (C.x * C.x + C.y * C.y) * (B.x - A.x)) / D;
                float2 circumcentre = new float2(circumcentreX, circumcentreY);

                // circumradius
                float a = math.distance(A, B), b = math.distance(B, C), c = math.distance(C, A);
                float circumradius = a * b * c / (4 * tri.area);

                tri.circumcentre = circumcentre;
                tri.circumradius = circumradius;
            }

            private void CalculateOrientation(ref JobTriangle tri)
            {
                var dir = math.normalize(tri.maxAnglePoint - tri.circumcentre);
                tri.normalOrientedDirection = dir;
                tri.orientedAngleDegree = (math.atan2(dir.y, dir.x) * k_Rad2Deg + 360f) % 360f; // [0:360]
            }
        }
    }
}