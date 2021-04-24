using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class FlowmapPathfinderMaster : MonoBehaviour
    {
        public LineRenderer pathLine;

        [HideInInspector]
        public Node previousNode;
        [HideInInspector]
        public List<Node> previousPath = new List<Node>();
        [HideInInspector]
        public List<Node> reachableNodes = new List<Node>();

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
            bool retVal = false;

            float xDistance = currentNode.worldPosition.x - targetNode.worldPosition.x;
            float zDistance = currentNode.worldPosition.z - targetNode.worldPosition.z;

            if (xDistance >= -1 && xDistance <= 1 && zDistance >= -1 && zDistance <= 1)
            {
                retVal = true;
            }

            return retVal;
        }
        public void CalculateNewPath(Vector3 origin, UnitController unitController)
        {
            Node currentNode = GridManager.instance.GetNode(origin, unitController.gridIndex);

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
                }
            }
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
        public void GetPathFromMap(Node origin, UnitController unitController)
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

                foreach (Node node in GetNeighbours(cn, unitController))
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

        //Seperated for loops to eliminate diagonals
        List<Node> GetNeighbours(Node currentNode, UnitController unitController)
        {
            List<Node> retVal = new List<Node>();

            for (int y = -unitController.verticalStepsDown; y <= unitController.verticalStepsUp; y++)
            {
                int _y = currentNode.position.y + y;

                for (int x = -1; x <= 1; x++)
                {
                    int _x = currentNode.position.x + x;
                    int _z = currentNode.position.z;

                    Node node = GridManager.instance.GetNode(_x, _y, _z, unitController.gridIndex);
                    if (node != null)
                    {
                        retVal.Add(node);
                    }
                }

                for (int z = -1; z <= 1; z++)
                {
                    int _x = currentNode.position.x;
                    int _z = currentNode.position.z + z;

                    Node node = GridManager.instance.GetNode(_x, _y, _z, unitController.gridIndex);
                    if (node != null)
                    {
                        retVal.Add(node);
                    }
                }
            }
            return retVal;
        }
        public void CalculateWalkablePositions()
        {
            if (BattleManager.instance.currentUnit == null)
                return;

            FlowmapPathfinder flowmapPathfinder =
                new FlowmapPathfinder(GridManager.instance, BattleManager.instance.currentUnit);

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