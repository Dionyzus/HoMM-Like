using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class PathfinderMaster : MonoBehaviour
    {
        public static PathfinderMaster instance;
        public LineRenderer pathLine;

        public delegate void PathIsFound(List<Node> path, GridUnit.OnPathReachCallback callback);
        PathIsFound onPathIsFound;
        GridUnit.OnPathReachCallback onPathReach;

        [HideInInspector]
        public List<Node> CreatedNodes = new List<Node>();

        List<Node> storedPath = new List<Node>();
        public List<Node> StoredPath { get => storedPath; set => storedPath = value; }

        private void Awake()
        {
            instance = this;
        }
        public void RequestPathAndPreview(Node start, Node target, GridUnit unit)
        {
            Pathfinder pathfinder = new Pathfinder(start, target, GridManager.instance, unit);
            storedPath = pathfinder.FindAndPreviewPath();
        }
        public void RequestPathfinder(PathIsFound onPathIsFound, GridUnit.OnPathReachCallback onPathReach)
        {
            this.onPathIsFound = onPathIsFound;
            this.onPathReach = onPathReach;

            GetStoredPath();
        }
        void GetStoredPath()
        {
            onPathIsFound?.Invoke(storedPath, onPathReach);
        }

        //Should check this out a little bit more
        public bool IsTargetNodeNeighbour(Node currentNode, Node targetNode)
        {
            float xDistance = Mathf.Abs(currentNode.worldPosition.x - targetNode.worldPosition.x);
            float zDistance = Mathf.Abs(currentNode.worldPosition.z - targetNode.worldPosition.z);

            if (xDistance <= 1 && zDistance <= 1)
            {
                return true;
            }

            return false;
        }

        public void ClearCreatedNodes()
        {
            foreach (Node node in CreatedNodes)
            {
                foreach (Node subNode in node.subNodes)
                {
                    GridManager.instance.ClearNode(subNode);
                }
                GridManager.instance.ClearNode(node);
            }

            CreatedNodes.Clear();
        }
    }
}