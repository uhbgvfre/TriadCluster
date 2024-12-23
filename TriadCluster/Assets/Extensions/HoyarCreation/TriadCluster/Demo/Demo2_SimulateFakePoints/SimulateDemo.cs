using System;
using System.Threading.Tasks;
using UnityEngine;

namespace HoyarCreation.TriadCluster.Demo
{
    public class SimulateDemo : MonoBehaviour
    {
        [Serializable]
        private enum CombinationQuantity
        {
            Seven,
            Twelve,
            Nineteen,
            NinetyOne
        }

        [Header("Gen policy")]
        [SerializeField] private CombinationQuantity combinationQuantity = CombinationQuantity.Twelve;

        [SerializeField] private bool isRandomPosition;

        [SerializeField] private PrepareForTest_GenDummyPoints prepareController;

        private async void Start()
        {
            await Task.Delay(1000);
            print("Start demo simulation");
            switch (combinationQuantity)
            {
                case CombinationQuantity.Seven:
                    prepareController.MakeAllPossibleTrianglesTemplate_15Degree();
                    break;
                case CombinationQuantity.Twelve:
                    prepareController.MakeAllPossibleTrianglesTemplate_12Degree();
                    break;
                case CombinationQuantity.Nineteen:
                    prepareController.MakeAllPossibleTrianglesTemplate_10Degree();
                    break;
                case CombinationQuantity.NinetyOne:
                    prepareController.MakeAllPossibleTrianglesTemplate_05Degree();
                    break;
                default: break;
            }

            await Task.Delay(1000);
            if (isRandomPosition) prepareController.Setup_RandomPosition();
            else prepareController.Setup_BenchmarkPosition();
            print("Create fake points to simulate touchscreen points. if in practice, you can use TriadClusterTouchPointsPublisher prefab to emit touch points event");

            await Task.Delay(1000);
            prepareController.sendInputEventsByTriangleUnitPoints = true;
            print("Set sendInputEventsByTriangleUnitPoints to TRUE");
        }
    }
}