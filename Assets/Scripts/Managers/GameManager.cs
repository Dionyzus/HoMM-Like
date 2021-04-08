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

        GameObject movePath;
        GridUnit storeUnit;

        Node previousNode;

        public LineRenderer pathLine;
        bool unitIsMoving;

        bool waitForInteraction;

        public Material standardMaterial;
        public Material highlightedMaterial;
        public Material mouseOverMaterial;

        public static GameManager instance;

        private void Awake()
        {
            instance = this;
        }

        private void Update()
        {
            if(storeUnit != null)
            {
                if (storeUnit.IsInteracting)
                {
                    return;
                } else
                {
                    storeUnit = null;
                }
            }
            if (targetUnit != null)
            {
                if (targetUnit.IsInteracting)
                {
                    if (waitForInteraction == false)
                    {
                        waitForInteraction = true;
                    }

                    return;
                }
                else
                {
                    if (waitForInteraction)
                    {
                        waitForInteraction = false;
                        unitIsMoving = false;
                        calculatePath = true;
                    }
                }
            }

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
                if (targetUnit != null)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        targetUnit.PlayAttack();
                        ClearHighlightedNodes();
                        unitIsMoving = true;
                        return;
                    }
                }

                HandleMouse();
            }

            if (calculatePath)
            {
                CalculateWalkablePositions();
                calculatePath = false;
            }
        }

        void ClearReachableNodes()
        {
            foreach (Node n in reachableNodes)
            {
                foreach (Node sn in n.subNodes)
                {
                    GridManager.instance.ClearNode(sn);
                }

                GridManager.instance.ClearNode(n);
            }

            foreach (Node n in highlightedNodes)
            {
                if (n.renderer != null)
                {
                    Destroy(n.renderer.gameObject);
                }
            }
            highlightedNodes.Clear();

            GridManager.instance.ClearGrids();
        }

        void CalculateWalkablePositions()
        {
            if (targetUnit == null)
                return;

            FlowmapPathfinder flowmap = new FlowmapPathfinder(
                    GridManager.instance, targetUnit);

            ClearReachableNodes();

            reachableNodes = flowmap.CreateFlowmapForNode();

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
                                        GetPathFromMap(currentNode, targetUnit);
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
                    {
                        n.renderer.material = highlightedMaterial;
                    }
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

        void GetPathFromMap(Node origin, GridUnit gridUnit)
        {
            previousPath.Clear();

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(origin);
            previousPath.Add(origin);

            if (movePath == null)
            {
                movePath = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            }

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                int minSteps = currentNode.steps;

                foreach (Node n in GetNeighbours(currentNode, gridUnit))
                {
                    if (!closedSet.Contains(n))
                    {
                        if (!openSet.Contains(n))
                        {
                            if (reachableNodes.Contains(n))
                            {
                                if (n.steps < minSteps)
                                {
                                    //minSteps = n.steps;
                                    openSet.Add(n);
                                    previousPath.Add(n);
                                    break;
                                }
                            }
                        }
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
            }

            LoadNodesToPath(previousPath);
        }

        List<Node> GetNeighbours(Node currentNode, GridUnit gridUnit)
        {
            List<Node> retVal = new List<Node>();

            for (int y = -gridUnit.verticalStepsDown; y <= gridUnit.verticalStepsUp; y++)
            {
                int _y = currentNode.position.y + y;

                for (int x = -1; x <= 1; x++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        int _x = currentNode.position.x + x;
                        int _z = currentNode.position.z + z;

                        Node node = GridManager.instance.GetNode(_x, _y, _z, gridUnit.gridIndex);

                        if (node != null)
                        {
                            retVal.Add(node);
                        }
                    }
                }
            }

            return retVal;
        }

        void LoadNodesToPath(List<Node> nodes)
        {
            pathLine.positionCount = nodes.Count;

            for (int i = 0; i < nodes.Count; i++)
            {
                pathLine.SetPosition(i, nodes[i].worldPosition + Vector3.up * 0.3f);
            }
        }

        public void UnitDeath(GridUnit gridUnit)
        {
            if (targetUnit == gridUnit)
            {
                targetUnit = null;
            }

            storeUnit = gridUnit;
            calculatePath = true;
        }
    }
}