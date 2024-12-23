using System.Collections.Generic;

namespace HoyarCreation.TriadCluster
{
    public static class DBSCAN
    {
        private const int k_Flag_Unclassified = 0;
        private const int k_Flag_Noise = -1;

        public static List<int> Run(List<(float, float)> points, float eps, int minPts)
        {
            List<int> labels = new(new int[points.Count]); // Initialize all points as unclassified
            int clusterId = 0; // Cluster counter

            for (int i = 0; i < points.Count; i++)
            {
                if (labels[i] != k_Flag_Unclassified) // If the point has been visited, skip
                    continue;

                List<int> neighborIndexes = RegionQuery(points, points[i], eps);

                if (neighborIndexes.Count < minPts)
                    labels[i] = k_Flag_Noise;
                else
                {
                    clusterId++;
                    ExpandCluster(points, labels, i, neighborIndexes, clusterId, eps, minPts);
                }
            }

            return labels;
        }

        // Expands the cluster
        private static void ExpandCluster(List<(float, float)> points, List<int> labels, int pointIndex, List<int> neighborIndexes, int clusterId, float epsilon, int minPoints)
        {
            labels[pointIndex] = clusterId; // Add point to cluster

            for (int i = 0; i < neighborIndexes.Count; i++)
            {
                int neighborIndex = neighborIndexes[i];
                if (labels[neighborIndex] == k_Flag_Unclassified)
                {
                    labels[neighborIndex] = clusterId; // Mark neighbor as visited and belonging to cluster
                    List<int> neighborNeighborIndexes = RegionQuery(points, points[neighborIndex], epsilon);
                    if (neighborNeighborIndexes.Count >= minPoints)
                        neighborIndexes.AddRange(neighborNeighborIndexes);
                }
                if (labels[neighborIndex] == k_Flag_Noise)
                    labels[neighborIndex] = clusterId; // Change noise to border point
            }
        }

        // Returns neighbors within point's epsilon-neighborhood
        private static List<int> RegionQuery(List<(float, float)> points, (float, float) point, float epsilon)
        {
            var epsilonsq = epsilon * epsilon;
            List<int> neighbors = new List<int>();
            for (int i = 0; i < points.Count; i++)
            {
                if (SqrDistance(points[i], point) <= epsilonsq)
                    neighbors.Add(i);
            }

            return neighbors;

            static float SqrDistance((float, float) ptA, (float, float) ptB)
            {
                return (ptB.Item1 - ptA.Item1) * (ptB.Item1 - ptA.Item1) + (ptB.Item2 - ptA.Item2) * (ptB.Item2 - ptA.Item2);
            }
        }
    }

}