using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class Node
    {
        public Vector3Int position;
        public int gridIndex;
        public int scaleXZ;
        public Vector3 pivotPosition;
        public Vector3 worldPosition;
        public Renderer renderer;

        public int steps;

        public List<Node> subNodes = new List<Node>();

        public bool isWalkable = false;

        public bool IsWalkable()
        {
            bool retVal = isWalkable;

            if (gridIndex != 0)
            {
                for (int i = 0; i < subNodes.Count; i++)
                {
                    subNodes[i].UpdateWalkability();

                    if (subNodes[i].isWalkable == false)
                    {
                        retVal = false;
                        continue;
                    }
                }
            }

            return retVal;
        }
        public void CheckForObstacles()
        {
            Vector3 origin = worldPosition + GridManager.instance.readExtents;

            Collider[] colliders = Physics.OverlapBox(
                origin, GridManager.instance.readExtents / 2,
                Quaternion.identity, GridManager.ignoreForObstacles);

            Debug.DrawLine(origin, origin + GridManager.instance.readExtents / 2, Color.blue, 20);
            Debug.DrawLine(origin, origin - GridManager.instance.readExtents / 2, Color.yellow, 20);

            GridManager.visualizeNodes.Add(origin);

            for (int i = 0; i < colliders.Length; i++)
            {
                Debug.Log("Collider: " + colliders[i].name);
                isWalkable = false;
            }
        }

        public void UpdateWalkability()
        {
            Vector3 origin = worldPosition;
            origin.y += scaleXZ / 2;

            Debug.DrawRay(worldPosition, Vector3.down * scaleXZ, Color.red, 5);

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, scaleXZ))
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
    }
}