using UnityEngine;

namespace HoyarCreation.TriadCluster
{
    // Final client use this class to get matched triangles
    public class MatchedTriangle
    {
        public int idLabel;
        public Vector2 circumcentre; // Position
        public float circumradius; // Radius
        public Vector2[] verticesPositions; // 3 vertices position of triangle
        public Vector2 normalOrientedDirection;
        public float orientedAngleDegree;
        public int framestamp; // Last be analyzed framestamp, for remove checking

        public MatchedTriangle(JobTriangle jobTriangle, int framestamp)
        {
            idLabel = jobTriangle.idLabel;
            UpdateMetadatas(
                jobTriangle.circumcentre,
                jobTriangle.circumradius,
                new Vector2[3] { jobTriangle.pointA, jobTriangle.pointB, jobTriangle.pointC },
                jobTriangle.normalOrientedDirection,
                jobTriangle.orientedAngleDegree,
                framestamp);
        }

        public void UpdateMetadatas(Vector2 circumcentre, float circumradius, Vector2[] verticesPositions, Vector2 normalOrientedDirection, float orientedAngleDegree, int framestamp)
        {
            this.circumcentre = circumcentre;
            this.circumradius = circumradius;
            this.verticesPositions = verticesPositions;
            this.normalOrientedDirection = normalOrientedDirection;
            this.orientedAngleDegree = orientedAngleDegree;
            this.framestamp = framestamp;
        }
    }
}