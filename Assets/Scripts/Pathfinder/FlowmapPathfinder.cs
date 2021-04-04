﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class FlowmapPathfinder
    {
        GridManager gridManager;
        int gridIndex;
        int stepsCount;
        Node origin;

        public delegate void OnCompleteCallback(List<Node> reachableNodes);
        OnCompleteCallback onCompleteCallback;
        public FlowmapPathfinder(GridManager gridManager, int gridIndex, Node origin, int stepsCount, OnCompleteCallback onCompleteCallback = null)
        {
            this.gridManager = gridManager;
            this.gridIndex = gridIndex;
            this.origin = origin;
            this.stepsCount = stepsCount;
            this.onCompleteCallback = onCompleteCallback;
        }

        public List<Node> CreateFlowmapForNode()
        {
            List<Node> reachableNodes = new List<Node>();
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            origin.steps = 0;
            reachableNodes.Add(origin);
            openSet.Add(origin);
            //int stepsAmount = 0;

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                int steps = currentNode.steps;
                steps += 1;

                foreach (Node n in GetNeighbours(currentNode))
                {
                    if (!closedSet.Contains(n))
                    {
                        if (!openSet.Contains(n))
                        {
                            openSet.Add(n);
                            n.steps = steps;

                            if (n.steps <= stepsCount)
                            {
                                reachableNodes.Add(n);
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
                int _x = currentNode.position.x + x;
                int _z = currentNode.position.z;

                Node neighbour = gridManager.GetNode(_x, currentNode.position.y, _z, gridIndex);

                if (neighbour != null)
                {
                    if(neighbour.IsWalkable())
                        retVal.Add(neighbour);
                }
            }

            for (int z = -1; z <= 1; z++)
            {
                int _x = currentNode.position.x;
                int _z = currentNode.position.z + z;

                Node neighbour = gridManager.GetNode(_x, currentNode.position.y, _z, gridIndex);

                if (neighbour != null)
                {
                    if(neighbour.IsWalkable())
                        retVal.Add(neighbour);
                }
            }

            return retVal;
        }
    }
}