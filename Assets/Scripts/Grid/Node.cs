using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class Node
    {
        public Vector3Int position;
        public int gridIndex;
        public Vector3 pivotPosition;
        public Vector3 worldPosition;
        public Renderer renderer;

        public int steps;

        public List<Node> subNodes = new List<Node>();

        public bool isWalkable = true;

        public bool IsWalkable()
        {
            bool retVal = isWalkable;

            for (int i = 0; i < subNodes.Count; i++)
            {
                if (subNodes[i].isWalkable == false)
                {
                    retVal = false;
                    continue;
                }
            }

            return retVal;
        }
    }
}