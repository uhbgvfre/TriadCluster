using MessagePipe;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoyarCreation.TriadCluster
{
    [RequireComponent(typeof(TriadClusterResolver))]
    public class TriadClusterTransmitter : MonoBehaviour
    {
        private TriadClusterResolver m_ResolverCache;
        private TriadClusterResolver m_Resolver => !m_ResolverCache ? m_ResolverCache = GetComponent<TriadClusterResolver>() : m_ResolverCache;

        private JobTriangle[] m_MatchedJobTriangles; // Every frame fetched from resolver
        private readonly Dictionary<int, MatchedTriangle> m_ExistedTriangles = new();

        private IPublisher<OnTriangleDownEvent> m_OnTriangleDownEventSender => GlobalMessagePipe.GetPublisher<OnTriangleDownEvent>();
        private IPublisher<OnTriangleStayEvent> m_OnTriangleStayEventSender => GlobalMessagePipe.GetPublisher<OnTriangleStayEvent>();
        private IPublisher<OnTriangleUpEvent> m_OnTriangleUpEventSender => GlobalMessagePipe.GetPublisher<OnTriangleUpEvent>();

        public UnityEvent<OnTriangleUpEvent> onTriangleUp;
        public UnityEvent<OnTriangleStayEvent> onTriangleStay;
        public UnityEvent<OnTriangleDownEvent> onTriangleDown;

        private static int m_FrameCountForNoRespondsThreshold => TriadClusterConfig.FrameCountForNoRespondsThreshold;

        public Dictionary<int, MatchedTriangle> GetExistedTriangles() => m_ExistedTriangles;

        private void Update()
        {
            var frameCount = Time.frameCount;
            m_MatchedJobTriangles = m_Resolver.GetMatchedJobTriangles();
            HandleCurrentTrianglesUpdating(frameCount);
            HandleExpiredTrianglesChecking(frameCount);
            HandleExistedTrianglesStayEventRaising();
        }

        private void HandleCurrentTrianglesUpdating(int frameCount)
        {
            foreach (var tri in m_MatchedJobTriangles)
            {
                if (!m_ExistedTriangles.TryGetValue(tri.idLabel, out var triangle))
                {
                    var matchedTriangle = new MatchedTriangle(tri, frameCount);
                    m_ExistedTriangles.Add(tri.idLabel, matchedTriangle);
                    SendOnTriangleDownEvent(matchedTriangle);
                }
                else
                {
                    var triVerticesLocalPositions = new Vector2[] { tri.pointA, tri.pointB, tri.pointC };
                    triangle.UpdateMetadatas(
                        tri.circumcentre,
                        tri.circumradius,
                        triVerticesLocalPositions,
                        tri.normalOrientedDirection,
                        tri.orientedAngleDegree,
                        frameCount
                    );
                }
            }
        }

        private void HandleExpiredTrianglesChecking(int frameCount)
        {
            // Record expired tri
            var trisThatShouldBeRemoved = new Stack<int>();
            foreach (var triKVP in m_ExistedTriangles)
            {
                bool isExpired = frameCount - triKVP.Value.framestamp > m_FrameCountForNoRespondsThreshold;
                if (isExpired)
                {
                    trisThatShouldBeRemoved.Push(triKVP.Key);
                }
            }

            // Remove expired tri and raise remove event
            while (trisThatShouldBeRemoved.TryPop(out var id))
            {
                var tri = m_ExistedTriangles[id];
                m_ExistedTriangles.Remove(id);
                SendOnTriangleUpEvent(tri);
            }
        }

        private void HandleExistedTrianglesStayEventRaising()
        {
            foreach (var tri in m_ExistedTriangles.Values)
            {
                SendOnTriangleStayEvent(tri);
            }
        }

        private void SendOnTriangleDownEvent(MatchedTriangle tri)
        {
            var e = new OnTriangleDownEvent(tri);
            m_OnTriangleDownEventSender.Publish(new OnTriangleDownEvent(tri));
            onTriangleDown?.Invoke(e);
        }

        private void SendOnTriangleStayEvent(MatchedTriangle tri)
        {
            var e = new OnTriangleStayEvent(tri);
            m_OnTriangleStayEventSender.Publish(new OnTriangleStayEvent(tri));
            onTriangleStay?.Invoke(e);
        }

        private void SendOnTriangleUpEvent(MatchedTriangle tri)
        {
            var e = new OnTriangleUpEvent(tri);
            m_OnTriangleUpEventSender.Publish(e);
            onTriangleUp?.Invoke(e);
        }
    }
}