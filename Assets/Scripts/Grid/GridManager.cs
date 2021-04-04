using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GridManager : MonoBehaviour
    {
        public float scaleY = 2;
        public int[] scales = { 1, 2};

        Vector3Int[] gridSizes;
        List<Node[,,]> grids = new List<Node[,,]>();

        GameObject[] gridParents;

        Vector3 minPosition;

        public static GridManager instance;

        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            ReadLevel();

            GridUnit[] gridUnits = GameObject.FindObjectsOfType<GridUnit>();
            foreach (GridUnit unit in gridUnits)
            {
                Node n = GetNode(unit.startPosition, unit.gridIndex);
                unit.transform.position = n.worldPosition;
                unit.currentNode = n;
                Debug.Log("World position: " + n.worldPosition);
            }
        }

        void ReadLevel()
        {
            GridPosition[] gridPositions = GameObject.FindObjectsOfType<GridPosition>();

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = minX;
            float maxZ = maxX;
            float minY = minX;
            float maxY = maxX;

            for (int i = 0; i < gridPositions.Length; i++)
            {
                Transform t = gridPositions[i].transform;

                if (t.position.x < minX)
                {
                    minX = t.position.x;
                }

                if (t.position.x > maxX)
                {
                    maxX = t.position.x;
                }

                if (t.position.z < minZ)
                {
                    minZ = t.position.z;
                }

                if (t.position.z > maxZ)
                {
                    maxZ = t.position.z;
                }

                if (t.position.y < minY)
                {
                    minY = t.position.y;
                }

                if (t.position.y > maxY)
                {
                    maxY = t.position.y;
                }
            }

            minPosition = Vector3.zero;
            minPosition.x = minX;
            minPosition.y = minY;
            minPosition.z = minZ;

            gridParents = new GameObject[scales.Length];
            gridSizes = new Vector3Int[scales.Length];

            for (int i = 0; i < scales.Length; i++)
            {
                gridParents[i] = new GameObject("Parent for index " + i);

                gridSizes[i] = new Vector3Int
                {
                    x = Mathf.FloorToInt((maxX - minX) / scales[i]),
                    y = Mathf.FloorToInt((maxY - minY) / scaleY),
                    z = Mathf.FloorToInt((maxZ - minZ) / scales[i])
                };

                grids.Add(CreateGrid(gridSizes[i], scales[i], i));
            }
        }

        Node[,,] CreateGrid(Vector3Int gridSize, int scaleXZ, int gridIndex)
        {
            Node[,,] grid = new Node[gridSize.x + 1, gridSize.y + 1, gridSize.z + 1];

            for (int x = 0; x < gridSize.x + 1; x++)
            {
                for (int z = 0; z < gridSize.z + 1; z++)
                {
                    for (int y = 0; y < gridSize.y + 1; y++)
                    {
                        Node n = new Node();
                        n.position.x = x;
                        n.position.y = y;
                        n.position.z = z;
                        n.gridIndex = gridIndex;

                        n.pivotPosition.x = x * scaleXZ;
                        n.pivotPosition.y = y * scaleY;
                        n.pivotPosition.z = z * scaleXZ;
                        n.pivotPosition += minPosition;

                        grid[x, y, z] = n;

                        int targetGridIndex = gridIndex - 1;

                        if (targetGridIndex >= 0)
                        {
                            FindSubNodes(n, scaleXZ, targetGridIndex);

                            Debug.Log("Subnodes count: " + n.subNodes.Count);
                        }

                        CreateNode(n, scaleXZ, gridIndex);
                    }
                }
            }

            return grid;
        }

        void FindSubNodes(Node node, int currentScale, int targetGridIndex)
        {
            int steps = currentScale - scales[0];

            Vector3Int scaledPosition = node.position;
            scaledPosition.x *= currentScale;
            scaledPosition.z *= currentScale;

            Debug.Log("Steps: " + steps);

            for (int x = 0; x <= steps; x++)
            {
                for (int z = 0; z <= steps; z++)
                {
                    int _x = scaledPosition.x + x;
                    int _z = scaledPosition.z + z;

                    Node subNode = GetNode(_x, node.position.y, _z, targetGridIndex);
                    if (subNode != null)
                    {
                        if (!node.subNodes.Contains(subNode))
                        {
                            node.subNodes.Add(subNode);
                        }
                    }
                }
            }
        }

        void CreateNode(Node node, int scaleXZ, int gridIndex)
        {
            Vector3 targetPosition = node.pivotPosition;
            Vector3 nodeScale = (Vector3.one * 0.95f) * scaleXZ;

            targetPosition.x += nodeScale.x / 2;
            targetPosition.z += nodeScale.z / 2;

            node.worldPosition = targetPosition;

            if (gridIndex == 0)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(go.GetComponent<Collider>());

                go.transform.position = targetPosition;
                go.transform.eulerAngles = new Vector3(90, 0, 0);
                go.transform.parent = gridParents[gridIndex].transform;
                go.transform.localScale = nodeScale;

                node.renderer = go.GetComponentInChildren<Renderer>();
            }
        }

        public Node GetNode(Vector3 worldPosition, int gridIndex)
        {
            Vector3Int position = new Vector3Int
            {
                x = Mathf.RoundToInt(worldPosition.x / scales[gridIndex]),
                z = Mathf.RoundToInt(worldPosition.z / scales[gridIndex]),
                y = Mathf.RoundToInt(worldPosition.y / scaleY)
            };

            return GetNode(position, gridIndex);
        }

        public Node GetNode(Vector3Int position, int gridIndex)
        {
            return GetNode(position.x, position.y, position.z, gridIndex);
        }

        public Node GetNode(int x, int y, int z, int gridIndex)
        {
            Vector3Int size = gridSizes[gridIndex];

            if (x < 0 || y < 0 || z < 0
                || x > size.x
                || y > size.y
                || z > size.z)
            {
                return null;
            }

            return grids[gridIndex][x, y, z];
        }
    }
}