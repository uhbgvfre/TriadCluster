namespace HoyarCreation.TriadCluster
{
    public class OnTriangleUpEvent
    {
        public readonly int idLabel;
        public readonly MatchedTriangle matchedTriangle;

        public OnTriangleUpEvent(MatchedTriangle matchedTriangle)
        {
            this.idLabel = matchedTriangle.idLabel;
            this.matchedTriangle = matchedTriangle;
        }
    }
}
