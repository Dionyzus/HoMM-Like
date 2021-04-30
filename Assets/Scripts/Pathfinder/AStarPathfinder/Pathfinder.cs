﻿using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class Pathfinder
    {
        Node startNode;
        Node endNode;
        GridManager grid;
        GridUnit unit;

        List<Node> path = new List<Node>();

        public Pathfinder(Node start, Node target, GridManager gridManager, GridUnit unit)
        {
            startNode = start;
            endNode = target;
            grid = gridManager;
            this.unit = unit;
        }

        public List<Node> FindAndPreviewPath()
        {
            path.Clear();

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
                    path = RetracePath(startNode, currentNode);
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

            DrawPath(path);
            return path;
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

            path.Reverse();
            return path;
        }
        List<Node> GetNeighbours(Node from)
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

                    if (_x == startNode.position.x && _z == startNode.position.z)
                        continue;

                    Node node = grid.GetNode(_x, _y, _z, unit.gridIndex);
                    Node newNode = null;

                    if (node != null && node.IsWalkable())
                        newNode = GetNeighbour(node, from, GridManager.IGNORE_FOR_OBSTACLES);

                    if (newNode != null)
                    {
                        result.Add(newNode);
                    }
                }
            }
            return result;
        }
        public Node GetNeighbour(Node searchPosition, Node from, LayerMask layer)
        {
            Vector3 origin = from.worldPosition;
            Vector3 target = searchPosition.worldPosition;
            origin.y += 0.25f;
            target.y += 0.25f;

            if (!Physics.Linecast(origin, target, out RaycastHit hit, layer))
            {
                return searchPosition;
            }
            return null;
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
    }
}