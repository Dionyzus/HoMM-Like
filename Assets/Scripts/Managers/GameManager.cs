using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HOMM_BM
{
    public class GameManager : MonoBehaviour
    {
        public GridUnit targetUnit;

        public bool calculatePath;
        List<Node> highlightedNodes = new List<Node>();
        [HideInInspector]
        public List<Node> reachableNodes = new List<Node>();

        public Material standardMaterial;
        public Material highlightedMaterial;
        public Material mouseOverMaterial;

        [HideInInspector]
        public Node previousNode;

        public LineRenderer pathLine;
        [HideInInspector]
        public bool unitIsMoving;
        bool waitForInteraction;

        GridUnit storeUnit;
        MouseLogic currentMouseLogic;
        public MouseLogic selectMove;
        public MouseLogic targetNodeAction;
        public MouseLogic performAttack;

        public static GameManager instance;
        private void Awake()
        {
            instance = this;
        }
        private void Update()
        {
            if (storeUnit != null)
            {
                if (storeUnit.IsInteracting)
                    return;
                storeUnit = null;
            }

            if (unitIsMoving)
            {
                ClearReachableNodes();
                if (targetUnit.MovingOnPathFinished())
                {
                    pathLine.positionCount = 0;
                    previousPath.Clear();
                    unitIsMoving = false;

                    if(!targetUnit.IsHittingEnemy)
                        calculatePath = true;
                }
            }

            else
            {
                if (targetUnit != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                        if (Physics.Raycast(ray, out RaycastHit hit, 100))
                        {
                            if (EventSystem.current.IsPointerOverGameObject())
                            {
                                if (hit.transform.GetComponentInChildren<InteractionTrigger>())
                                {
                                    InteractionHook hook = hit.transform.GetComponentInParent<InteractionHook>();

                                    if (targetUnit != null)
                                    {
                                        if (hook != null)
                                        {
                                            targetUnit.currentInteractionHook = hook;
                                        }
                                        else
                                        {
                                            targetUnit.currentInteractionHook = null;
                                        }
                                    }
                                }
                                else if (hit.transform.gameObject.layer == GridManager.enemyUnitsLayer)
                                {
                                    targetUnit.IsHittingEnemy = true;
                                    targetUnit.CurrentEnemyTarget = hit.transform.GetComponentInChildren<GridUnit>();
                                    HandleMovingAction(targetUnit.CurrentNode.worldPosition);
                                    return;
                                }
                                else
                                {
                                    Node currentNode = GridManager.instance.GetNode(hit.point, targetUnit.gridIndex);
                                    if (!reachableNodes.Contains(currentNode))
                                    {
                                        //Should actually add check for distance between actual target and available steps
                                        //Add path to stored path, and calculate path from that node
                                        previousPath.Clear();
                                        calculatePath = true;
                                    }
                                }
                            }
                        }
                    }
                    if (targetUnit.currentInteractionHook != null && Input.GetKeyDown(KeyCode.Space))
                    {
                        InitiateMovingOnPath();
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (currentMouseLogic == null)
                        currentMouseLogic = selectMove;
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
            foreach (Node node in reachableNodes)
            {
                foreach (Node subNode in node.subNodes)
                {
                    GridManager.instance.ClearNode(subNode);
                }
                GridManager.instance.ClearNode(node);
            }

            foreach (Node node in highlightedNodes)
            {
                GridManager.instance.ClearNode(node);
            }

            highlightedNodes.Clear();

            GridManager.instance.ClearGrids();
        }
        void CalculateWalkablePositions()
        {
            if (targetUnit == null)
                return;

            FlowmapPathfinder flowmapPathfinder =
                new FlowmapPathfinder(GridManager.instance, targetUnit);

            ClearReachableNodes();

            reachableNodes = flowmapPathfinder.CreateFlowmapForNode();

            foreach (Node node in reachableNodes)
            {
                for (int i = 0; i < node.subNodes.Count; i++)
                {
                    if (node.subNodes[i].renderer != null)
                    {
                        highlightedNodes.Add(node.subNodes[i]);
                        node.subNodes[i].renderer.material = highlightedMaterial;
                    }
                }

                if (node.renderer != null)
                {
                    highlightedNodes.Add(node);
                    node.renderer.material = highlightedMaterial;
                }
            }
        }

        void HandleMouse()
        {
            if (targetUnit != null && targetUnit.IsInteracting)
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                if (currentMouseLogic != null)
                    currentMouseLogic.InteractTick(this, hit);
            }
        }
        public void ClearHighlightedNodes()
        {
            if (previousNode != null)
            {
                foreach (Node node in previousNode.subNodes)
                {
                    if (node.renderer != null)
                        node.renderer.material = highlightedMaterial;
                }

                if (previousNode.renderer != null)
                {
                    previousNode.renderer.material = highlightedMaterial;
                }
            }

            highlightedNodes.Clear();
        }

        public void HighlightNodes(Node currentNode)
        {
            ClearHighlightedNodes();
            previousNode = currentNode;

            foreach (Node node in currentNode.subNodes)
            {
                if (node.renderer != null)
                    node.renderer.material = mouseOverMaterial;
            }

            if (currentNode.renderer != null)
            {
                currentNode.renderer.material = mouseOverMaterial;
            }
        }
        [HideInInspector]
        public List<Node> previousPath = new List<Node>();

        public void GetPathFromMap(Node origin, GridUnit gridUnit)
        {
            previousPath.Clear();

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(origin);
            previousPath.Add(origin);

            while (openSet.Count > 0)
            {
                Node cn = openSet[0];
                int minStep = cn.steps;

                foreach (Node node in GetNeighbours(cn, gridUnit))
                {
                    if (!closedSet.Contains(node))
                    {
                        if (!openSet.Contains(node))
                        {
                            if (reachableNodes.Contains(node))
                            {
                                if (node.steps < minStep)
                                {
                                    openSet.Add(node);
                                    previousPath.Add(node);
                                    break;
                                }
                            }
                        }
                    }
                }
                openSet.Remove(cn);
                closedSet.Add(cn);
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

        public void LoadNodesToPath(List<Node> nodes)
        {
            pathLine.positionCount = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {
                pathLine.SetPosition(i, nodes[i].worldPosition + Vector3.up * .3f);
            }
        }

        public void OnSelectCurrentUnit(GridUnit gridUnit)
        {
            if (unitIsMoving)
                return;

            if (targetUnit == gridUnit)
                return;

            if (gridUnit != null)
            {
                pathLine.positionCount = 0;
                targetUnit = gridUnit;
                currentMouseLogic = selectMove;

                UiManager.instance.OnCharacterSelected(targetUnit);

                if (currentMouseLogic != null)
                    currentMouseLogic.InteractTick(this, targetUnit);
            }
        }

        public void UnitDeath(GridUnit gridUnit)
        {
            if (targetUnit == gridUnit)
                targetUnit = null;

            storeUnit = gridUnit;
            calculatePath = true;
        }
        public void HandleMovingAction(Vector3 origin)
        {
            if (targetUnit != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    InitiateMovingOnPath();
                }
                Node currentNode = GridManager.instance.GetNode(origin, targetUnit.gridIndex);
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
        public void InitiateMovingOnPath()
        {
            if (previousPath.Count > 0)
            {
                ClearHighlightedNodes();
                targetUnit.LoadPathAndStartMoving(previousPath);
                unitIsMoving = true;
            }
        }
    }
}