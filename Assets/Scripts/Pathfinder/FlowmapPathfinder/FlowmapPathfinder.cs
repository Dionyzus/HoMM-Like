using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HOMM_BM
{
    public class FlowmapPathfinder
    {
        GridManager gridManager;
        UnitController unitController;

        public delegate void OnCompleteCallback(List<Node> reachableNodes);
        OnCompleteCallback onCompleteCallback;

        public FlowmapPathfinder(GridManager gridManager, UnitController unitController, OnCompleteCallback onCompleteCallback = null)
        {
            this.gridManager = gridManager;
            this.unitController = unitController;
            this.onCompleteCallback = onCompleteCallback;
        }
        public List<Node> CreateFlowmapForNode()
        {
            List<Node> reachableNodes = new List<Node>();
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            unitController.CurrentNode.steps = 0;

            reachableNodes.Add(unitController.CurrentNode);
            openSet.Add(unitController.CurrentNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                int steps = currentNode.steps;
                steps++;

                if (steps <= unitController.StepsCount)
                {
                    foreach (Node node in GetNeighbours(currentNode, steps))
                    {
                        if (!closedSet.Contains(node))
                        {
                            if (!openSet.Contains(node))
                            {
                                openSet.Add(node);
                                node.steps = steps;

                                if (node.steps <= unitController.StepsCount)
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

        List<Node> GetNeighbours(Node currentNode, int steps)
        {
            List<Node> retVal = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0)
                        continue;

                    //Restrict main diagonal to -1 of maximum unit steps
                    if (x == z && steps == unitController.StepsCount - 1)
                    {
                        continue;
                    }
                    if (x == -1 && z == 1 && steps == unitController.StepsCount - 1)
                    {
                        continue;
                    }
                    if (x == 1 && z == -1 && steps == unitController.StepsCount - 1)
                    {
                        continue;
                    }

                    int _x = x + currentNode.position.x;
                    int _y = currentNode.position.y;
                    int _z = z + currentNode.position.z;

                    if (_x == currentNode.position.x && _z == currentNode.position.z)
                        continue;

                    Node node = gridManager.GetNode(_x, _y, _z, unitController.GridIndex);

                    if (_x == unitController.CurrentNode.position.x &&
                                _z == unitController.CurrentNode.position.z)
                    {
                        retVal.Add(unitController.CurrentNode);
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