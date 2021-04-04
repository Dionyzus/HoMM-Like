using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GridUnit : MonoBehaviour, ISelectable
    {
        public int gridIndex = 0;
        public Vector3Int startPosition;
        public Node CurrentNode
        {
            get
            {
                return GridManager.instance.GetNode(transform.position, gridIndex);
            }
        }
        public int stepsCount = 3;

        List<Node> currentPath = new List<Node>();
        float time;
        int index;
        bool isPathInitialized;
        float actualMovementSpeed;

        public float movementSpeed = 2f;

        Vector3 originPosition;
        Vector3 targetPosition;


        public GridUnit GetGridUnit()
        {
            return this;
        }

        public bool MovingOnPath()
        {
            bool isFinished = false;

            if (!isPathInitialized)
            {
                originPosition = CurrentNode.worldPosition;
                targetPosition = currentPath[index].worldPosition;
                time = 0;

                float distance = Vector3.Distance(originPosition, targetPosition);
                actualMovementSpeed = movementSpeed / distance;

                Vector3 direction = targetPosition - originPosition;
                direction.y = 0;

                if(direction == Vector3.zero)
                {
                    direction = transform.forward;
                }
                transform.rotation = Quaternion.LookRotation(direction);

                isPathInitialized = true;
            }

            time += Time.deltaTime * actualMovementSpeed;

            if (time > 1)
            {
                isPathInitialized = false;

                index += 1;
                if (index > currentPath.Count - 1)
                {
                    time = 1;
                    isFinished = true;
                }
            }

            transform.position = Vector3.Lerp(originPosition, targetPosition, time);

            return isFinished;
        }

        public void LoadPathAndStartMoving(List<Node> path)
        {
            isPathInitialized = false;
            index = 0;
            time = 0;

            path.Reverse();
            currentPath = path;
        }
    }
}
