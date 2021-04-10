using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class GridUnit : MonoBehaviour, ISelectable
    {
        public int gridIndex = 0;
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
        Vector3 originPosition;
        Vector3 targetPosition;
        public GridAction currentGridAction;

        public float movementSpeed = 2;
        float actualMovementSpeed;

        bool isWalking;
        bool isDirty;

        public GameObject onDeathEnable;

        bool moveIsBasedOnAnimation;
        float animationLength;

        public bool isUnitDeadDebug;
        public bool IsInteracting
        {
            get
            {
                return animator.GetBool("isInteracting");
            }
        }

        Animator animator;
        public AnimatorOverrideController overrideController;
        Collider unitCollider;

        void Awake()
        {
            gameObject.layer = 8;
        }
        void Start()
        {
            animator = GetComponentInChildren<Animator>();
            if (overrideController != null)
                animator.runtimeAnimatorController = overrideController;

            animator.applyRootMotion = false;
            unitCollider = GetComponentInChildren<Collider>();
        }
        void Update()
        {
            if (isUnitDeadDebug)
            {
                enabled = false;
                KillUnit();
                isUnitDeadDebug = false;
                return;
            }

            animator.applyRootMotion = IsInteracting;
            animator.SetBool("isWalking", isWalking);

            if (IsInteracting)
            {
                Vector3 deltaPosition = animator.deltaPosition;
                transform.position += deltaPosition;

                animator.transform.localPosition = Vector3.zero;
                if (!isDirty)
                    isDirty = true;
            }
            else
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
        public void PlayAnimation(string animation)
        {
            animator.applyRootMotion = true;
            animator.Play(animation);
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

                if (moveIsBasedOnAnimation)
                {
                    actualMovementSpeed = (distance / animationLength) / distance; //0-1
                }

                Vector3 direction = targetPosition - originPosition;
                direction.y = 0;
                if (direction == Vector3.zero)
                {
                    direction = transform.forward;
                }
                transform.rotation = Quaternion.LookRotation(direction);

                isPathInitialized = true;
                isWalking = true;
            }

            if (!moveIsBasedOnAnimation)
            {
                time += Time.deltaTime * actualMovementSpeed;
            }
            else
            {
                if (animator.GetBool("isMoving"))
                {
                    time += Time.deltaTime * actualMovementSpeed;
                }
                else
                {
                    if (time > 0)
                    {
                        time = 1;
                    }
                }
            }

            if (time > 1)
            {
                isPathInitialized = false;

                index++;
                if (index > currentPath.Count - 1)
                {
                    time = 1;
                    isFinished = true;
                    isWalking = false;
                    moveIsBasedOnAnimation = false;
                }
            }

            transform.position = Vector3.Lerp(originPosition, targetPosition, time);

            return isFinished;
        }

        public void LoadPathAndStartMoving(List<Node> path, bool reverse = true)
        {
            moveIsBasedOnAnimation = false;
            isPathInitialized = false;
            index = 0;
            time = 0;

            if (reverse)
                path.Reverse();
            currentPath = path;
        }
        public void LoadGridActionToMove(List<Node> path, AnimationClip animationClip)
        {
            isPathInitialized = false;
            index = 0;
            time = 0;
            moveIsBasedOnAnimation = true;
            animationLength = animationClip.length;
            currentPath = path;
        }
        public void KillUnit()
        {
            animator.applyRootMotion = true;
            animator.Play("Dying");
            animator.SetBool("isInteracting", true);
            unitCollider.enabled = false;

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                c.gameObject.layer = 9;
            }
            animator.transform.parent = null;

            if (onDeathEnable != null)
            {
                onDeathEnable.SetActive(true);
            }
            GameManager.instance.UnitDeath(this);
        }
    }
}