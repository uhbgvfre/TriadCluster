using System;
using System.Collections;
using UnityEngine;

namespace HoyarCreation.TriadCluster.Demo
{
    public class MinimalUseCaseExample : MonoBehaviour
    {
        public bool drawTriangleGizmoOnTriangleStay = true;

        private IDisposable m_DisposableEvents;

        private TriadClusterTransmitter m_TriadClusterTransmitter;

        private IEnumerator Start()
        {
            m_TriadClusterTransmitter = FindAnyObjectByType<TriadClusterTransmitter>();

            var wait1S = new WaitForSeconds(1);
            while (Application.isPlaying)
            {
                yield return wait1S;
                var existedTriangles = m_TriadClusterTransmitter?.GetExistedTriangles()?.Values;
                if (existedTriangles == null) continue;
                print($"frame:{Time.frameCount} | existedTriangles.Count: {existedTriangles.Count}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawTriangleGizmoOnTriangleStay) return;
            Gizmos.color = Color.green;

            var existedTriangles = m_TriadClusterTransmitter?.GetExistedTriangles()?.Values;
            if (existedTriangles == null) return;

            foreach (var x in existedTriangles)
            {
                // Draw triangle
                Gizmos.DrawLine(x.verticesPositions[0], x.verticesPositions[1]);
                Gizmos.DrawLine(x.verticesPositions[1], x.verticesPositions[2]);
                Gizmos.DrawLine(x.verticesPositions[2], x.verticesPositions[0]);

                // Draw direction
                Gizmos.DrawLine(x.circumcentre, x.circumcentre + x.circumradius * x.normalOrientedDirection);

                // Draw center point
                Gizmos.DrawWireSphere(x.circumcentre, 10f);
            }
        }
    }
}