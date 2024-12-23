using System.Collections.Generic;
using UnityEngine;

namespace HoyarCreation.TriadCluster
{
    public class UpdateInputPointsEvent
    {
        public readonly List<Vector2> inputPoints;

        public UpdateInputPointsEvent(List<Vector2> inputPoints)
        {
            this.inputPoints = inputPoints;
        }
    }
}
