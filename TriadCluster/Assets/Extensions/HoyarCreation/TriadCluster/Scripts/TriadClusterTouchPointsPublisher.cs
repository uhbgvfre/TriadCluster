
using MessagePipe;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HoyarCreation.TriadCluster
{
    // Auto send UpdateInputPointsEvent according to touchscreen touches
    public class TriadClusterTouchPointsPublisher : MonoBehaviour
    {
        [SerializeField] private List<Vector2> m_DummyTouchPoints = new List<Vector2>(); // For test on editor
        private IPublisher<UpdateInputPointsEvent> m_UpdateInputPointsEventSender => GlobalMessagePipe.GetPublisher<UpdateInputPointsEvent>();

        private void Update()
        {
            var touchPts = Input.touches.Select(x => x.position).ToList();

            if (m_DummyTouchPoints != null && m_DummyTouchPoints.Count > 0)
            {
                touchPts.AddRange(m_DummyTouchPoints);
            }

            var e = new UpdateInputPointsEvent(touchPts);
            m_UpdateInputPointsEventSender.Publish(e);
        }
    }
}