using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class Node
    {
        public Vector3Int position;
        public int gridIndex;
        public int scaleXZ;
        public int steps;

        public Vector3 pivotPosition;
        public Vector3 worldPosition;
        public Renderer renderer;

        public List<Node> subNodes = new List<Node>();

        public bool isWalkable = false;

        public void UpdateWalkability()
        {
            Vector3 origin = worldPosition;
            origin.y += GridManager.instance.groundDistanceOffset;

            float distance = GridManager.instance.groundDistanceCheck;

            //Debug.DrawRay(origin, Vector3.down * distance, Color.red, 5);

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance, GridManager.ignoreForObstacles))
            {
                if (hit.transform.gameObject.layer == 9)
                {
                    isWalkable = true;
                }
                worldPosition = hit.point;
            }

            if (isWalkable)
                CheckForObstacles();
        }
        public bool IsWalkable()
        {
            bool retVal = isWalkable;

            if (gridIndex != 0)
            {
                if (subNodes.Count == scaleXZ * 2)
                {
                    retVal = true;

                    for (int i = 0; i < subNodes.Count; i++)
                    {
                        subNodes[i].UpdateWalkability();

                        if (subNodes[i].isWalkable == false)
                        {
                            retVal = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                UpdateWalkability();
            }
            return retVal;
        }

        public void CheckForObstacles()
        {
            Vector3 origin = worldPosition + GridManager.instance.readOffset;

            Collider[] colliders = Physics.OverlapBox(origin,
                GridManager.instance.readExtents / 2, Quaternion.identity, GridManager.ignoreForObstacles);

            //Debug.DrawLine(origin, origin + GridManager.instance.readExtents / 2, Color.blue, 20);
            //Debug.DrawLine(origin, origin - GridManager.instance.readExtents / 2, Color.yellow, 20);

            GridManager.visualizedNodes.Add(origin);

            for (int i = 0; i < colliders.Length; i++)
            {
                isWalkable = false;
            }
        }
    }
}