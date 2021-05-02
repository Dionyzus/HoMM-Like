using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    //Check if it is possible to clear some nodes, cause it looks like some are not cleared.
    public class FlowmapPathfinderMaster : MonoBehaviour
    {
        public LineRenderer pathLine;

        [HideInInspector]
        public Node previousNode;
        [HideInInspector]
        public List<Node> previousPath = new List<Node>();
        [HideInInspector]
        public List<Node> reachableNodes = new List<Node>();
        [HideInInspector]
        public List<Node> UnwalkableNodes = new List<Node>();

        public Material standardMaterial;
        public Material highlightedMaterial;
        public Material mouseOverMaterial;

        List<Node> highlightedNodes = new List<Node>();

        public static FlowmapPathfinderMaster instance;

        private void Awake()
        {
            instance = this;
        }
        //Should check this out a little bit more
        public bool IsTargetNodeNeighbour(Node currentNode, Node targetNode)
        {
            float xDistance = Mathf.Abs(currentNode.worldPosition.x - targetNode.worldPosition.x);
            float zDistance = Mathf.Abs(currentNode.worldPosition.z - targetNode.worldPosition.z);

            if (xDistance <= 1 && zDistance <= 1)
            {
                return true;
            }
            return false;
        }
        public void CalculateNewPath(Vector3 origin, UnitController unitController)
        {
            Node currentNode = GridManager.instance.GetNode(origin, unitController.GridIndex);

            if (currentNode != null)
            {
                if (reachableNodes.Contains(currentNode))
                {
                    if (currentNode.IsWalkable())
                    {
                        if (previousNode != currentNode)
                        {
                            HighlightNodes(currentNode);
                            GetPathFromMap(currentNode, unitController);
                        }
                    }
                    else
                    {
                        UnwalkableNodes.Add(currentNode);
                    }
                }
                else
                {
                    UnwalkableNodes.Add(currentNode);
                }
            }
        }
        public void ClearUnwalkableNodes()
        {
            foreach (Node node in UnwalkableNodes)
            {
                foreach (Node subNode in node.subNodes)
                {
                    GridManager.instance.ClearNode(subNode);
                }
                GridManager.instance.ClearNode(node);
            }

            UnwalkableNodes.Clear();
        }
        public void ClearFlowmapData()
        {
            ClearHighlightedNodes();
            ClearReachableNodes();
        }
        public void InitiateMovingOnPath(UnitController unitController)
        {
            if (previousPath.Count > 0)
            {
                ClearHighlightedNodes();

                unitController.LoadPathAndStartMoving(previousPath);
                BattleManager.instance.unitIsMoving = true;

                ClearReachableNodes();
            }
        }
        int GetDistance(Node positionA, Node positionB)
        {
            int distanceX = Mathf.Abs(positionA.position.x - positionB.position.x);
            int distanceZ = Mathf.Abs(positionA.position.z - positionB.position.z);

            if (distanceX > distanceZ)
            {
                return 14 * distanceZ + 10 * (distanceX - distanceZ);
            }

            return 14 * distanceX + 10 * (distanceZ - distanceX);
        }

        List<Node> RetracePath(Node start, Node end)
        {
            List<Node> path = new List<Node>();
            Node currentNode = end;

            while (currentNode != start)
            {
                path.Add(currentNode);
                currentNode = currentNode.parentNode;
            }

            return path;
        }
        public void GetPathFromMap(Node origin, UnitController unitController)
        {
            previousPath.Clear();

            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(unitController.CurrentNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];

                for (int i = 0; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost ||
                       (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    {
                        if (!currentNode.Equals(openSet[i]))
                        {
                            currentNode = openSet[i];
                        }
                    }
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode.Equals(origin))
                {
                    previousPath = RetracePath(unitController.CurrentNode, currentNode);
                    break;
                }

                foreach (Node neighbour in GetNeighbours(currentNode, unitController))
                {
                    if (!closedSet.Contains(neighbour))
                    {
                        float moveCost = currentNode.gCost + GetDistance(currentNode, neighbour);
                        if (moveCost < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = moveCost;
                            neighbour.hCost = GetDistance(neighbour, origin);
                            neighbour.parentNode = currentNode;
                            if (!openSet.Contains(neighbour))
                            {
                                openSet.Add(neighbour);
                            }
                        }
                    }
                }
            }

            if (BattleManager.instance.currentUnit.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                LoadNodesToPath(previousPath);
        }

        List<Node> GetNeighbours(Node from, UnitController unit)
        {
            List<Node> result = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0)
                        continue;

                    int _x = x + from.position.x;
                    int _y = from.position.y;
                    int _z = z + from.position.z;

                    if (_x == from.position.x && _z == from.position.z)
                        continue;

                    Node node = GridManager.instance.GetNode(_x, _y, _z, unit.GridIndex);

                    if (_x == unit.CurrentNode.position.x &&
                            _z == unit.CurrentNode.position.z)
                    {
                        result.Add(unit.CurrentNode);
                    }
                    else if (node != null && node.IsWalkable())
                    {
                        result.Add(node);
                    }
                }
            }
            return result;
        }
        public void CalculateWalkablePositions()
        {
            if (BattleManager.instance.currentUnit == null)
                return;

            FlowmapPathfinder flowmapPathfinder =
                new FlowmapPathfinder(GridManager.instance, BattleManager.instance.currentUnit);

            ClearReachableNodes();

            reachableNodes = flowmapPathfinder.CreateFlowmapForNode();

            if (BattleManager.instance.currentUnit.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
            {
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
        public void ClearPathData()
        {
            if (pathLine.positionCount > 0)
                pathLine.positionCount = 0;

            previousPath.Clear();
            ClearHighlightedNodes();
        }
        public void LoadNodesToPath(List<Node> nodes)
        {
            pathLine.positionCount = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {
                pathLine.SetPosition(i, nodes[i].worldPosition + Vector3.up * .3f);
            }
        }
    }
}