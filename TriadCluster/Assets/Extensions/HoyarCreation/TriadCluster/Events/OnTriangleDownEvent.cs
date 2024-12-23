namespace HoyarCreation.TriadCluster
{
    public class OnTriangleDownEvent
    {
        public readonly int idLabel;
        public readonly MatchedTriangle matchedTriangle;

        public OnTriangleDownEvent(MatchedTriangle matchedTriangle)
        {
            this.idLabel = matchedTriangle.idLabel;
            this.matchedTriangle = matchedTriangle;
        }
    }
}