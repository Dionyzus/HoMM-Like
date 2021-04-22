using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class Pathfinder
    {
        public delegate void PathIsFound(List<Node> path, GridUnit.OnPathReachCallback callback);
        PathIsFound callback;
        GridUnit.OnPathReachCallback onPathReachCallback;
        Node startNode;
        Node endNode;
        GridManager grid;
        GridUnit unit;
        public Pathfinder(Node start, Node target, PathIsFound callback, GridUnit.OnPathReachCallback onPathReachCallback, GridManager gridManager, GridUnit unit)
        {
            startNode = start;
            endNode = target;
            grid = gridManager;
            this.unit = unit;
            this.callback = callback;
            this.onPathReachCallback = onPathReachCallback;
        }
        public void FindPath()
        {
            callback?.Invoke(FindPathActual(), onPathReachCallback);
        }
        List<Node> FindPathActual()
        {
            List<Node> foundPath = new List<Node>();
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

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

                if (currentNode.Equals(endNode))
                {
                    foundPath = RetracePath(startNode, currentNode);
                    break;
                }

                foreach (Node neighbour in GetNeighbours(currentNode))
                {
                    if (!closedSet.Contains(neighbour))
                    {
                        float moveCost = currentNode.gCost + GetDistance(currentNode, neighbour);
                        if (moveCost < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = moveCost;
                            neighbour.hCost = GetDistance(neighbour, endNode);
                            neighbour.parentNode = currentNode;
                            if (!openSet.Contains(neighbour))
                            {
                                openSet.Add(neighbour);
                            }
                        }
                    }
                }
            }

            DrawPath(foundPath);

            return foundPath;
        }
        public void DrawPath(List<Node> path)
        {
            PathfinderMaster.instance.pathLine.positionCount = path.Count;

            int reversedIndex = path.Count - 1;

            for (int i = 0; i < path.Count; i++)
            {
                PathfinderMaster.instance.pathLine.SetPosition(reversedIndex, path[i].worldPosition + Vector3.up * .3f);
                reversedIndex--;
            }
        }
        int GetDistance(Node positionA, Node positionB)
        {
            int distanceX = Mathf.Abs(positionA.x - positionB.x);
            int distanceZ = Mathf.Abs(positionA.z - positionB.z);

            if (distanceX > distanceZ)
            {
                return 14 * distanceZ + 10 * (distanceX - distanceZ);
            }

            return 14 * distanceX + 10 * (distanceZ - distanceX);
        }

        List<Node> GetNeighbours(Node currentNode)
        {
            List<Node> retVal = new List<Node>();

            for (int y = -1; y <= 1; y++)
            {
                int _y = currentNode.position.y + y;

                for (int x = -1; x <= 1; x++)
                {
                    int _x = currentNode.position.x + x;
                    int _z = currentNode.position.z;

                    Node neighbour = grid.GetNode(_x, _y, _z, unit.gridIndex);
                    if (neighbour != null)
                    {
                        if (neighbour.IsWalkable())
                            retVal.Add(neighbour);
                    }
                }

                for (int z = -1; z <= 1; z++)
                {
                    int _x = currentNode.position.x;
                    int _z = currentNode.position.z + z;

                    Node neighbour = grid.GetNode(_x, _y, _z, unit.gridIndex);
                    if (neighbour != null)
                    {
                        if (neighbour.IsWalkable())
                            retVal.Add(neighbour);
                    }
                }
            }
            return retVal;
        }

        List<Node> RetracePath(Node start, Node end)
        {
            List<Node> path = new List<Node>();
            //Could be mistake, end is unused
            Node currentNode = end;

            while (currentNode != start)
            {
                path.Add(currentNode);
                currentNode = currentNode.parentNode;
            }

            path.Reverse();
            return path;
        }
    }
}