using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GameManager : MonoBehaviour
    {
        public GridUnit targetUnit;
        public bool calculatePath;

        List<Node> previousHighlightedNodes = new List<Node>();

        public Material standardMaterial;
        public Material highlightedMaterial;

        private void Update()
        {
            if (calculatePath)
            {
                CalculateWalkablePositions();
                calculatePath = false;
            }
        }

        void CalculateWalkablePositions()
        {
            if (targetUnit == null)
                return;

            FlowmapPathfinder flowmap = new FlowmapPathfinder(
                    GridManager.instance,
                    targetUnit.gridIndex,
                    targetUnit.currentNode,
                    targetUnit.stepsCount);

            List<Node> reachableNodes = flowmap.CreateFlowmapForNode();

            foreach (Node n in previousHighlightedNodes)
            {
                if (n.renderer != null)
                {
                    n.renderer.material = standardMaterial;
                }
            }
            previousHighlightedNodes.Clear();

            foreach (Node n in reachableNodes)
            {
                for (int i = 0; i < n.subNodes.Count; i++)
                {
                    if (n.subNodes[i].renderer != null)
                    {
                        previousHighlightedNodes.Add(n.subNodes[i]);
                        n.subNodes[i].renderer.material = highlightedMaterial;
                    }
                }

                if (n.renderer != null)
                {
                    previousHighlightedNodes.Add(n);
                    n.renderer.material = highlightedMaterial;
                }
            }
        }
    }
}