using MessagePipe;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace HoyarCreation.TriadCluster.Demo
{
    public class UseCase_ResolveSuitablePointsToMatchedTriangles : MonoBehaviour
    {
        public bool printUnitOnTriangleDown = true;
        public bool printUnitOnTriangleUp = true;
        public bool drawTriangleGizmoOnTriangleStay = true;

        private ISubscriber<OnTriangleDownEvent> m_OnTriangleDown => GlobalMessagePipe.GetSubscriber<OnTriangleDownEvent>();
        private ISubscriber<OnTriangleUpEvent> m_OnTriangleUp => GlobalMessagePipe.GetSubscriber<OnTriangleUpEvent>();
        private ISubscriber<OnTriangleStayEvent> m_OnTriangleStay => GlobalMessagePipe.GetSubscriber<OnTriangleStayEvent>();

        private IDisposable m_DisposableEvents;

        private List<MatchedTriangle> m_MatchedTriangles = new();

        private void Start()
        {
            var disposables = DisposableBag.CreateBuilder();
            m_OnTriangleDown.Subscribe(OnTriangleDown).AddTo(disposables);
            m_OnTriangleUp.Subscribe(OnTriangleUp).AddTo(disposables);
            m_OnTriangleStay.Subscribe(OnTriangleStay).AddTo(disposables);
            m_DisposableEvents = disposables.Build();
        }

        private void OnDestroy() => m_DisposableEvents.Dispose();

        private void OnTriangleDown(OnTriangleDownEvent e)
        {
            var tri = e.matchedTriangle;
            m_MatchedTriangles.Add(tri);

            if (!printUnitOnTriangleDown) return;
            print($"[TriDown] id:{tri.idLabel} pos: {tri.circumcentre} r: {tri.circumradius} pt1: {tri.verticesPositions[0]} pt2: {tri.verticesPositions[1]} pt3: {tri.verticesPositions[2]}");
        }

        private void OnTriangleUp(OnTriangleUpEvent e)
        {
            var tri = e.matchedTriangle;
            m_MatchedTriangles.Remove(tri);

            if (!printUnitOnTriangleUp) return;
            print($"[TriUp] id:{tri.idLabel} pos: {tri.circumcentre} r: {tri.circumradius} pt1: {tri.verticesPositions[0]} pt2: {tri.verticesPositions[1]} pt3: {tri.verticesPositions[2]}");
        }

        private void OnTriangleStay(OnTriangleStayEvent e)
        {
            // Do something with event args,
            // or use triadClusterTransmitter.GetExistedTriangles() to get stay matched triangles.
        }

        private void OnDrawGizmos()
        {
            if (!drawTriangleGizmoOnTriangleStay) return;
            Gizmos.color = Color.green;
            m_MatchedTriangles.ForEach(x =>
            {
                // Draw triangle
                Gizmos.DrawLine(x.verticesPositions[0], x.verticesPositions[1]);
                Gizmos.DrawLine(x.verticesPositions[1], x.verticesPositions[2]);
                Gizmos.DrawLine(x.verticesPositions[2], x.verticesPositions[0]);

                // Draw direction
                Gizmos.DrawLine(x.circumcentre, x.circumcentre + x.circumradius * x.normalOrientedDirection);

                // Draw center point
                Gizmos.DrawWireSphere(x.circumcentre, 10f);
            });
        }
    }
}