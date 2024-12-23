using Unity.Mathematics;

namespace HoyarCreation.TriadCluster
{
    public struct JobTriangle
    {
        public float2 pointA;
        public float2 pointB;
        public float2 pointC;

        // Populate in precalc phase
        public float2 maxAnglePoint;
        public float angleDegreeMax;
        public float angleDegreeMid;
        public float angleDegreeMin;
        public float area;

        // Populate in match & postcalc phase
        public int idLabel;
        public float2 circumcentre;
        public float circumradius;
        public float2 normalOrientedDirection;
        public float orientedAngleDegree;
    }
}