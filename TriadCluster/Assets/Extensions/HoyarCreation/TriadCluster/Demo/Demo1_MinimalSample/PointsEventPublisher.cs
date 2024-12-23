using System.Collections.Generic;
using MessagePipe;
using UnityEngine;

namespace HoyarCreation.TriadCluster.Demo
{
    public class PointsEventPublisher : MonoBehaviour
    {
        [SerializeField] private List<Transform> points;
        private static IPublisher<UpdateInputPointsEvent> m_EventSender_UpdateInputPoints => GlobalMessagePipe.GetPublisher<UpdateInputPointsEvent>();

        private void Update()
        {
            List<Vector2> allPoints = new(points.Count);
            foreach (var p in points)
            {
                allPoints.Add(p.position);
            }

            m_EventSender_UpdateInputPoints.Publish(new UpdateInputPointsEvent(allPoints));
        }

        private void OnDisable()
        {
            print("OnDisable: Send empty points to clear");
            m_EventSender_UpdateInputPoints.Publish(new UpdateInputPointsEvent(null));
        }

        private void OnDrawGizmos()
        {
            if (points == null || points.Count == 0) return;

            Gizmos.color = Color.blue;
            foreach (var p in points)
            {
                Gizmos.DrawWireSphere(p.position, 20f);
            }
        }
    }
}