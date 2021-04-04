using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GridUnit : MonoBehaviour
    {
        public int gridIndex = 0;
        public Vector3Int startPosition;
        public Node currentNode;
        public int stepsCount = 3;
    }
}
