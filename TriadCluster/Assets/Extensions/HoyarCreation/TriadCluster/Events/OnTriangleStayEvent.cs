namespace HoyarCreation.TriadCluster
{
    public class OnTriangleStayEvent
    {
        public readonly int idLabel;
        public readonly MatchedTriangle matchedTriangle;

        public OnTriangleStayEvent(MatchedTriangle matchedTriangle)
        {
            this.idLabel = matchedTriangle.idLabel;
            this.matchedTriangle = matchedTriangle;
        }
    }
}