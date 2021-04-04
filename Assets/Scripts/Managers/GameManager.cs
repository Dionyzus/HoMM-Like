using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GameManager : MonoBehaviour
    {
        public GridUnit targetUnit;
        public bool calculatePath;

        List<Node> highlightedNodes = new List<Node>();
        List<Node> reachableNodes = new List<Node>();

        List<Node> previousPath = new List<Node>();

        Node previousNode;

        public LineRenderer pathLine;
        bool unitIsMoving;

        public Material standardMaterial;
        public Material highlightedMaterial;
        public Material mouseOverMaterial;

        private void Update()
        {
            if (unitIsMoving)
            {
                if (targetUnit.MovingOnPath())
                {
                    unitIsMoving = false;
                    calculatePath = true;
                }
            }
            else
            {
                if (calculatePath)
                {
                    CalculateWalkablePositions();
                    calculatePath = false;
                }

                HandleMouse();
            }
        }

        void CalculateWalkablePositions()
        {
            if (targetUnit == null)
                return;

            FlowmapPathfinder flowmap = new FlowmapPathfinder(
                    GridManager.instance,
                    targetUnit.gridIndex,
                    targetUnit.CurrentNode,
                    targetUnit.stepsCount);

            reachableNodes = flowmap.CreateFlowmapForNode();

            foreach (Node n in highlightedNodes)
            {
                if (n.renderer != null)
                {
                    n.renderer.material = standardMaterial;
                }
            }
            highlightedNodes.Clear();

            foreach (Node n in reachableNodes)
            {
                for (int i = 0; i < n.subNodes.Count; i++)
                {
                    if (n.subNodes[i].renderer != null)
                    {
                        highlightedNodes.Add(n.subNodes[i]);
                        n.subNodes[i].renderer.material = highlightedMaterial;
                    }
                }

                if (n.renderer != null)
                {
                    highlightedNodes.Add(n);
                    n.renderer.material = highlightedMaterial;
                }
            }
        }

        void HandleMouse()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                ISelectable selectable = hit.transform.GetComponentInParent<ISelectable>();
                if (selectable != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (selectable.GetGridUnit() != targetUnit)
                        {
                            targetUnit = selectable.GetGridUnit();
                            calculatePath = true;
                        }
                    }
                }
                else
                {
                    if (targetUnit != null)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            if (previousPath.Count > 0)
                            {
                                ClearHighlightedNodes();
                                targetUnit.LoadPathAndStartMoving(previousPath);
                                unitIsMoving = true;
                            }
                        }
                        Node currentNode = GridManager.instance.GetNode(hit.point, targetUnit.gridIndex);
                        if (currentNode != null)
                        {
                            if (reachableNodes.Contains(currentNode))
                            {
                                if (currentNode.IsWalkable())
                                {
                                    if (previousNode != currentNode)
                                    {
                                        HighlightNodes(currentNode);
                                        GetPathFromMap(currentNode, targetUnit.CurrentNode, targetUnit.gridIndex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void ClearHighlightedNodes()
        {
            if (previousNode != null)
            {
                foreach (Node n in previousNode.subNodes)
                {
                    if (n.renderer != null)
                        n.renderer.material = highlightedMaterial;
                }

                if (previousNode.renderer != null)
                {
                    previousNode.renderer.material = highlightedMaterial;
                }
            }
        }

        void HighlightNodes(Node currentNode)
        {
            ClearHighlightedNodes();

            previousNode = currentNode;

            foreach (Node n in currentNode.subNodes)
            {
                if (n.renderer != null)
                    n.renderer.material = mouseOverMaterial;
            }

            if (currentNode.renderer != null)
            {
                currentNode.renderer.material = mouseOverMaterial;
            }
        }

        void GetPathFromMap(Node currentNode, Node targetNode, int gridIndex)
        {
            previousPath.Clear();

            Node startNode = currentNode;
            previousPath.Add(currentNode);
            bool isPathValid = true;

            while (currentNode != targetNode)
            {
                currentNode = GetNeighbour(currentNode, currentNode.steps, gridIndex);

                if (currentNode == null)
                {
                    isPathValid = false;
                    break;
                }

                previousPath.Add(currentNode);

                if (startNode == currentNode)
                {
                    break;
                }
            }

            Debug.Log(previousPath.Count);
            if (isPathValid)
            {
                LoadNodesToPath(previousPath);
            }
            else
            {
                ClearPathNodes();
                previousPath.Clear();
            }
        }

        Node GetNeighbour(Node currentNode, int steps, int gridIndex)
        {
            Node retVal = null;
            int minSteps = steps;

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    int _x = currentNode.position.x + x;
                    int _z = currentNode.position.z + z;

                    Node node = GridManager.instance.GetNode(_x, currentNode.position.y, _z, gridIndex);

                    if (node != null)
                    {
                        if (node.steps < minSteps)
                        {
                            minSteps = node.steps;
                            retVal = node;
                        }
                    }
                }
            }

            return retVal;
        }

        void ClearPathNodes()
        {
            pathLine.positionCount = 0;
        }

        void LoadNodesToPath(List<Node> nodes)
        {
            pathLine.positionCount = nodes.Count;

            for (int i = 0; i < nodes.Count; i++)
            {
                pathLine.SetPosition(i, nodes[i].worldPosition + Vector3.up * 0.3f);
            }
        }
    }
}