using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GridUnit : MonoBehaviour, ISelectable
    {
        Animator animator;
        public AnimatorOverrideController overrideController;

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
        public int verticalStepsUp = 1;
        public int verticalStepsDown = 3;

        List<Node> currentPath = new List<Node>();
        float time;
        int index;
        bool isPathInitialized;
        float actualMovementSpeed;

        bool isWalking;

        bool isDirty;

        public float movementSpeed = 2f;

        Vector3 originPosition;
        Vector3 targetPosition;
        
        public bool IsInteracting
        {
            get
            {
                return animator.GetBool("isInteracting");
            }
        }


        private void Awake()
        {
            gameObject.layer = 8;
        }
        private void Start()
        {
            animator = GetComponentInChildren<Animator>();

            if (overrideController != null)
                animator.runtimeAnimatorController = overrideController;

            animator.applyRootMotion = false;
        }
        private void Update()
        {
            animator.applyRootMotion = IsInteracting;
            animator.SetBool("isWalking", isWalking);

            if (IsInteracting)
            {
                Vector3 deltaPosition = animator.deltaPosition;
                transform.position += deltaPosition;

                animator.transform.localPosition = Vector3.zero;

                if (!isDirty)
                {
                    isDirty = true;
                }
            } else
            {
                if (isDirty)
                {
                    isDirty = false;

                    transform.position = CurrentNode.worldPosition;
                }
            }
        }
        public GridUnit GetGridUnit()
        {
            return this;
        }

        public void PlayAttack()
        {
            animator.Play("Jump Attack");
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
                isWalking = true;
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
                    isWalking = false;
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
