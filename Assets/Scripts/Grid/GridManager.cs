using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GridManager : MonoBehaviour
    {
        //Maybe change this to property
        public Vector3 readExtents = new Vector3(.25f, .5f, .25f);
        public Vector3 readOffset = new Vector3(0, .5f, 0);
        //Can be property
        public static List<Vector3> visualizeNodes = new List<Vector3>();
        public bool visualizeCollisions = true;

        public float scaleY = 2;
        public int[] scales = { 1, 2 };

        Vector3Int[] gridSizes;
        List<Node[,,]> grids = new List<Node[,,]>();

        GameObject[] gridParents;

        Vector3 minPosition;

        GameObject collisionObject;

        //Maybe change this to property
        public static GridManager instance;

        public static LayerMask ignoreForObstacles;

        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            ignoreForObstacles = ~(1 << 8);

            ReadLevel();

            //collisionObject = new GameObject("Collision");
            //collisionObject.AddComponent<BoxCollider>();

            //Node middleNode = GetNode(gridSizes[0].x / 2, 0, gridSizes[0].z / 2, 0);
            //collisionObject.transform.position = middleNode.worldPosition;

            //Vector3 targetScale = Vector3.one * 1000;
            //targetScale.y = 0.03f;

            //collisionObject.transform.localScale = targetScale;

            Debug.Log("Grids: " + grids);

            GridUnit[] gridUnits = GameObject.FindObjectsOfType<GridUnit>();
            foreach (GridUnit unit in gridUnits)
            {
                Node n = GetNode(unit.startPosition, unit.gridIndex);
                unit.transform.position = n.worldPosition;
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

                grids.Add(CreateGrid(gridSizes[i]));
            }
        }

        Node[,,] CreateGrid(Vector3Int gridSize)
        {
            Debug.Log("Grid size: " + gridSize.y);
            return new Node[gridSize.x + 1, gridSize.y + 1, gridSize.z + 1];
        }

        void CreateNodeOnGrid(Vector3Int position, int gridIndex)
        {
            int scaleXZ = scales[gridIndex];

            Node n = new Node
            {
                position = position,
                gridIndex = gridIndex,
                scaleXZ = scaleXZ
            };

            n.pivotPosition.x = n.position.x * scaleXZ;
            n.pivotPosition.y = n.position.y * scaleY;
            n.pivotPosition.z = n.position.z * scaleXZ;
            n.pivotPosition += minPosition;

            Vector3 nodeScale = (Vector3.one * 0.95f) * scaleXZ;
            Vector3 targetPosition = n.pivotPosition;

            targetPosition.x += nodeScale.x / 2;
            targetPosition.z += nodeScale.z / 2;

            n.worldPosition = targetPosition;

            grids[gridIndex][n.position.x, n.position.y, n.position.z] = n;

            int targetGridIndex = gridIndex - 1;

            if (targetGridIndex >= 0)
            {
                FindSubNodes(n, scaleXZ, targetGridIndex);

                Debug.Log("Subnodes count: " + n.subNodes.Count);
            }
            else
            {
                n.UpdateWalkability();

            }

            if (n.IsWalkable())
            {
                CreateNodeReference(n, scaleXZ, gridIndex, nodeScale);
            }
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

        void CreateNodeReference(Node node, int scaleXZ, int gridIndex, Vector3 nodeScale)
        {
            //Vector3 origin = node.worldPosition;
            //origin.y += scaleY / 2;

            //Debug.DrawRay(node.worldPosition, Vector3.down * scaleY, Color.red, 5);
            //if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, .5f))
            //{
            //    node.worldPosition = hit.point;
            //}

            if (gridIndex == 0)
            {
                //if (node.isWalkable == false)
                //    return;

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(go.GetComponent<Collider>());

                go.transform.position = node.worldPosition;
                go.transform.eulerAngles = new Vector3(90, 0, 0);
                go.transform.parent = gridParents[gridIndex].transform;
                go.transform.localScale = nodeScale;

                node.renderer = go.GetComponentInChildren<Renderer>();
            }
        }

        public bool IsPositionInsideGrid(Vector3 worldPosition, int gridIndex) 
        {
            Vector3Int position = new Vector3Int();

            worldPosition -= minPosition;

            position.x = Mathf.RoundToInt(worldPosition.x / scales[gridIndex]);
            position.y = Mathf.RoundToInt(worldPosition.y / scaleY);
            position.z = Mathf.RoundToInt(worldPosition.z / scales[gridIndex]);

            Vector3Int size = gridSizes[gridIndex];

            if (position.x < 0 || position.y < 0 || position.z < 0
                || position.x > size.x
                || position.y > size.y
                || position.z > size.z)
            {
                return false;
            }

            return true;
        }

        public Node GetNode(Vector3 worldPosition, int gridIndex)
        {
            worldPosition -= minPosition;

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

            if (grids[gridIndex][x, y, z] == null)
            {
                CreateNodeOnGrid(new Vector3Int(x, y, z), gridIndex);
            }

            return grids[gridIndex][x, y, z];
        }

        private void OnDrawGizmos()
        {
            if (visualizeCollisions)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < visualizeNodes.Count; i++)
                {
                    Gizmos.DrawWireCube(visualizeNodes[i], readExtents);
                }
            }
        }
    }
}