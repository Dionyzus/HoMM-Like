using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GridManager : MonoBehaviour
    {
        public float scaleY = 2;
        public float[] scales = { 1, 2};

        Vector3Int[] gridSizes;
        List<Node[,,]> grids = new List<Node[,,]>();

        Vector3 minPosition;
        Transform gridParent;

        private void Start()
        {
            ReadLevel();

            GridUnit[] gridUnits = GameObject.FindObjectsOfType<GridUnit>();
            foreach (GridUnit unit in gridUnits)
            {
                Node n = GetNode(unit.startPosition, unit.gridIndex);
                unit.transform.position = n.worldPosition;
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

            gridParent = new GameObject("Grid Parent").transform;
            gridParent.position = minPosition;

            gridSizes = new Vector3Int[scales.Length];

            for (int i = 0; i < scales.Length; i++)
            {
                gridSizes[i] = new Vector3Int
                {
                    x = Mathf.FloorToInt((maxX - minX) / scales[i]),
                    y = Mathf.FloorToInt((maxY - minY) / scaleY),
                    z = Mathf.FloorToInt((maxZ - minZ) / scales[i])
                };

                grids.Add(CreateGrid(gridSizes[i], scales[i]));
            }
        }

        Node[,,] CreateGrid(Vector3Int gridSize, float scaleXZ)
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

                        n.pivotPosition.x = x * scaleXZ;
                        n.pivotPosition.y = y * scaleY;
                        n.pivotPosition.z = z * scaleXZ;
                        n.pivotPosition += minPosition;

                        float scaleDifference = scaleXZ / 2;
                        n.worldPosition = n.pivotPosition;
                        n.worldPosition.x += scaleDifference;
                        n.worldPosition.z += scaleDifference;

                        grid[x, y, z] = n;

                        CreateNode(n, scaleXZ);
                    }
                }
            }

            return grid;
        }

        void CreateNode(Node node, float scaleXZ)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(gameObject.GetComponent<Collider>());

            Vector3 targetPosition = node.pivotPosition;
            gameObject.transform.localScale = (Vector3.one * 0.95f) * scaleXZ;

            targetPosition.x += gameObject.transform.localScale.x / 2;
            targetPosition.z += gameObject.transform.localScale.z / 2;

            gameObject.transform.position = targetPosition;
            gameObject.transform.eulerAngles = new Vector3(90, 0, 0);
            gameObject.transform.parent = gridParent;
        }

        Node GetNode(Vector3 worldPosition, int gridIndex)
        {
            Vector3Int position = new Vector3Int
            {
                x = Mathf.RoundToInt(worldPosition.x / scales[gridIndex]),
                z = Mathf.RoundToInt(worldPosition.z / scales[gridIndex]),
                y = Mathf.RoundToInt(worldPosition.y / scaleY)
            };

            return GetNode(position, gridIndex);
        }

        Node GetNode(Vector3Int position, int gridIndex)
        {
            Vector3Int size = gridSizes[gridIndex];

            if(position.x < 0 || position.y < 0 || position.z < 0
                || position.x > size.x 
                || position.y > size.y 
                || position.z > size.z)
            {
                return null;
            }

            return grids[gridIndex][position.x, position.y, position.z];
        }
    }
}