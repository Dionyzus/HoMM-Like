using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HOMM_BM
{
    public class FlowmapPathfinder
    {
        GridManager gridManager;
        GridUnit gridUnit;

        public delegate void OnCompleteCallback(List<Node> reachableNodes);
        OnCompleteCallback onCompleteCallback;

        public FlowmapPathfinder(GridManager gridManager, GridUnit gridUnit, OnCompleteCallback onCompleteCallback = null)
        {
            this.gridManager = gridManager;
            this.gridUnit = gridUnit;
            this.onCompleteCallback = onCompleteCallback;
        }
        public List<Node> CreateFlowmapForNode()
        {
            List<Node> reachableNodes = new List<Node>();
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            gridUnit.CurrentNode.steps = 0;

            reachableNodes.Add(gridUnit.CurrentNode);
            openSet.Add(gridUnit.CurrentNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                int steps = currentNode.steps;
                steps++;

                if (steps <= gridUnit.StepsCount)
                {
                    foreach (Node node in GetNeighbours(currentNode))
                    {
                        if (!closedSet.Contains(node))
                        {
                            if (!openSet.Contains(node))
                            {
                                openSet.Add(node);
                                node.steps = steps;

                                if (node.steps <= gridUnit.StepsCount)
                                    reachableNodes.Add(node);
                            }
                        }
                    }
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
            }
            return reachableNodes;
        }

        List<Node> GetNeighbours(Node currentNode)
        {
            List<Node> retVal = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0)
                        continue;

                    int _x = x + currentNode.position.x;
                    int _y = currentNode.position.y;
                    int _z = z + currentNode.position.z;

                    if (_x == currentNode.position.x && _z == currentNode.position.z)
                        continue;

                    Node node = gridManager.GetNode(_x, _y, _z, gridUnit.GridIndex);

                    if (_x == gridUnit.CurrentNode.position.x &&
                                _z == gridUnit.CurrentNode.position.z)
                    {
                        retVal.Add(gridUnit.CurrentNode);
                    }
                    else if (node != null && node.IsWalkable())
                    {
                        retVal.Add(node);
                    }
                    else if (node != null)
                    {
                        FlowmapPathfinderMaster.instance.UnwalkableNodes.Add(node);
                    }
                }
            }
            return retVal;
        }
    }
}