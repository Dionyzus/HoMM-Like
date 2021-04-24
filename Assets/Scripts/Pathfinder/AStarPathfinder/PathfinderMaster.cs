using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace HOMM_BM
{
    public class PathfinderMaster : MonoBehaviour
    {
        public static PathfinderMaster instance;
        public LineRenderer pathLine;

        private void Awake()
        {
            instance = this;
        }
        public void RequestPathPreview(Node start, Node target, GridUnit unit)
        {
            Pathfinder pathfinder = new Pathfinder(start, target, GridManager.instance, unit);
            pathfinder.PreviewPath();
        }
        public void RequestPathfinder(Node start, Node target, Pathfinder.PathIsFound onPathIsFound, GridUnit.OnPathReachCallback onPathReach, GridUnit unit)
        {
            Pathfinder pathfinder = new Pathfinder(start, target, onPathIsFound, onPathReach, GridManager.instance, unit);
            pathfinder.FindPath();
        }

        //Should check this out a little bit more
        public bool IsTargetNodeNeighbour(Node currentNode, Node targetNode)
        {
            bool retVal = false;

            float xDistance = currentNode.worldPosition.x - targetNode.worldPosition.x;
            float zDistance = currentNode.worldPosition.z - targetNode.worldPosition.z;

            if (xDistance >= -1 && xDistance <= 1 && zDistance >= -1 && zDistance <= 1)
            {
                retVal = true;
            }

            return retVal;
        }
    }
}