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
        public void RequestPathfinder(Node start, Node target, Pathfinder.PathIsFound onPathIsFound, GridUnit.OnPathReachCallback onPathReach, GridUnit unit)
        {
            Pathfinder pathfinder = new Pathfinder(start, target, onPathIsFound, onPathReach, GridManager.instance, unit);
            pathfinder.FindPath();
        }
    }
}