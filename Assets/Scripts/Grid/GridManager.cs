using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GridManager : MonoBehaviour
    {
        public Vector3 readExtents = new Vector3(.5f, .5f, .5f);
        public Vector3 readOffset = new Vector3(0, .5f, 0);
        public float groundDistanceCheck = .2f;
        public float groundDistanceOffset = .2f;

        public float scaleY = 2;
        public int[] scales = { 1, 2 };
        Vector3Int[] gridSizes;
        List<Node[,,]> grids = new List<Node[,,]>();
        GameObject[] gridParents;
        //First position is bottom left !y=0!, second position is upper right !y=1! -> Battle
        //First position is bottom left !y=0!, second position is upper right !y>=2! -> World
        GridPosition[] gridPositions;

        Vector3 minPosition;

        public static LayerMask IGNORE_FOR_OBSTACLES;

        public static int ENEMY_UNITS_LAYER = 10;
        public static int FRIENDLY_UNITS_LAYER = 8;

        public static GridManager instance;

        public Vector3 MinPosition { get => minPosition; set => minPosition = value; }
        public GameObject[] GridParents { get => gridParents; set => gridParents = value; }
        public GridPosition[] GridPositions { get => gridPositions; set => gridPositions = value; }

        private int maxSize;
        public int MaxSize
        {
            get
            {
                return gridSizes[0].x * gridSizes[0].z;
            }
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            IGNORE_FOR_OBSTACLES = ~(1 << 8);
            ReadLevel();
        }

        void ReadLevel()
        {
            gridPositions = FindObjectsOfType<GridPosition>();

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = minX;
            float maxY = maxX;
            float minZ = minX;
            float maxZ = maxX;

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
                if (t.position.y < minY)
                {
                    minY = t.position.y;
                }
                if (t.position.y > maxY)
                {
                    maxY = t.position.y;
                }
                if (t.position.z < minZ)
                {
                    minZ = t.position.z;
                }
                if (t.position.z > maxZ)
                {
                    maxZ = t.position.z;
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
                gridParents[i] = new GameObject("Parent for index: " + i);

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
            Node[,,] grid = new Node[gridSize.x + 1, gridSize.y + 1, gridSize.z + 1];

            return grid;
        }

        void CreateNodeOnGrid(Vector3Int position, int gridIndex, bool isSimulation = false)
        {
            int scaleXZ = scales[gridIndex];

            Node node = new Node
            {
                position = position,
                gridIndex = gridIndex,
                scaleXZ = scaleXZ
            };

            node.pivotPosition.x = node.position.x * scaleXZ;
            node.pivotPosition.y = node.position.y * scaleY;
            node.pivotPosition.z = node.position.z * scaleXZ;
            node.pivotPosition += minPosition;

            Vector3 nodeScale = (Vector3.one * 0.95f) * scaleXZ;
            Vector3 targetPosition = node.pivotPosition;

            targetPosition.x += nodeScale.x / 2;
            targetPosition.z += nodeScale.z / 2;
            node.worldPosition = targetPosition;

            grids[gridIndex][node.position.x, node.position.y, node.position.z] = node;
            int targetGridIndex = gridIndex - 1;

            if (!isSimulation && targetGridIndex >= 0)
            {
                FindSubNodes(node, scaleXZ, 0);
            }
            else
            {
                node.UpdateWalkability();
            }

            if (!isSimulation && node.IsWalkable())
                CreateNodeReferences(node, gridIndex, nodeScale);
        }

        void CreateNodeReferences(Node node, int scaleIndex, Vector3 nodeScale)
        {
            if (scaleIndex == 0)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Destroy(go.GetComponent<Collider>());
                Vector3 targetPosition = node.worldPosition;
                targetPosition.y += .01f;
                go.transform.position = targetPosition;
                go.transform.eulerAngles = new Vector3(90, 0, 0);
                go.transform.parent = gridParents[scaleIndex].transform;
                go.transform.localScale = nodeScale;

                node.renderer = go.GetComponentInChildren<Renderer>();
                node.renderer.material = FlowmapPathfinderMaster.instance.standardMaterial;
            }
        }

        void FindSubNodes(Node node, int currentScale, int targetGridIndex)
        {
            int steps = currentScale - scales[0];

            Vector3Int scaledPosition = node.position;
            scaledPosition.x *= currentScale;
            scaledPosition.z *= currentScale;

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
        public bool IsPositionInsideGrid(Vector3 worldPosition, int gridIndex)
        {
            Vector3Int position = new Vector3Int();
            worldPosition -= minPosition;

            position.x = Mathf.RoundToInt(worldPosition.x / scales[gridIndex]);
            position.z = Mathf.RoundToInt(worldPosition.z / scales[gridIndex]);
            position.y = Mathf.RoundToInt(worldPosition.y / scaleY);

            Vector3Int size = gridSizes[gridIndex];

            if (position.x < 0 || position.y < 0 || position.z < 0 ||
                position.x > size.x || position.y > size.y || position.z > size.z)
            {
                return false;
            }

            return true;
        }
        public Node GetNode(Vector3 worldPosition, int gridIndex)
        {
            Vector3Int position = new Vector3Int();
            worldPosition -= minPosition;

            position.x = Mathf.RoundToInt(worldPosition.x / scales[gridIndex]);
            position.z = Mathf.RoundToInt(worldPosition.z / scales[gridIndex]);
            position.y = Mathf.RoundToInt(worldPosition.y / scaleY);

            return GetNode(position, gridIndex);
        }
        public Node GetNode(Vector3Int position, int gridIndex)
        {
            return GetNode(position.x, position.y, position.z, gridIndex);
        }

        public Node GetNode(int x, int y, int z, int gridIndex, bool isSimulation = false)
        {
            Vector3Int size = gridSizes[gridIndex];

            if (x < 0 || y < 0 || z < 0 || x > size.x
                || y > size.y || z > size.z)
            {
                return null;
            }
            if (grids[gridIndex][x, y, z] == null)
            {
                CreateNodeOnGrid(new Vector3Int(x, y, z), gridIndex, isSimulation);
            }
            return grids[gridIndex][x, y, z];
        }

        public void ClearNode(Node node)
        {
            if (node.renderer != null)
            {
                Destroy(node.renderer.gameObject);
            }
        }

        public void ClearGrids()
        {
            for (int i = 0; i < grids.Count; i++)
            {
                grids[i] = CreateGrid(gridSizes[i]);
            }
        }
    }
}