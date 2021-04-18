﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        public string actionAnimation;
        public AnimationClip animationClip;

        List<Node> currentPath = new List<Node>();
        float time;
        int index;
        bool isPathInitialized;
        Vector3 originPosition;
        Vector3 targetPosition;
        public GridAction currentGridAction;

        public RenderTexture unitImage;

        public float movementSpeed = 2;
        float actualMovementSpeed;

        bool isWalking;
        bool isDirty;

        public GameObject onDeathEnable;

        private bool isHittingEnemy;
        public bool IsHittingEnemy { get => isHittingEnemy; set => isHittingEnemy = value; }

        private GridUnit currentEnemyTarget;
        public GridUnit CurrentEnemyTarget { get => currentEnemyTarget; set => currentEnemyTarget = value; }

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
        public Animator Animator
        {
            get
            {
                return animator;
            }
        }

        private Slider interactionSlider;
        public Slider InteractionSlider { get => interactionSlider; set => interactionSlider = value; }

        public AnimatorOverrideController overrideController;
        Collider unitCollider;

        public int stackIndex;
        public InteractionInstance currentInteractionInstance;
        public List<InteractionInstance> interactionInstances = new List<InteractionInstance>();

        [HideInInspector]
        public InteractionHook currentInteractionHook;

        Interaction currentInteraction;

        public int stepsCountSlider;

        float deltaTime;

        void Awake()
        {
            //Maybe seperate enemy and friendly units
            //gameObject.layer = 8;
        }

        void Start()
        {
            animator = GetComponentInChildren<Animator>();
            if (overrideController != null)
                animator.runtimeAnimatorController = overrideController;

            animator.applyRootMotion = false;
            isHittingEnemy = false;

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

            //Check this out...
            //animator.applyRootMotion = IsInteracting;
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


            deltaTime = Time.deltaTime;

            if (currentInteraction != null)
            {
                //HandleInteraction(currentInteraction, deltaTime);
                HandleUnitAttack(currentInteraction, deltaTime);
            }
            if (currentInteraction == null)
            {
                if (interactionInstances.Count > 0)
                {
                    LoadInteractionStack(interactionInstances[0]);
                    interactionInstances.RemoveAt(0);
                }
            }
        }

        public GridUnit GetGridUnit()
        {
            return this;
        }
        public void PlayAnimation(string animation, bool applyRootMotion = false)
        {
            animator.applyRootMotion = applyRootMotion;
            animator.Play(animation);
        }
        public bool MovingOnPathFinished()
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
                GameManager.instance.pathLine.positionCount -= 1;
                SetInteractionSliderStatus();

                if (index > currentPath.Count - 1)
                {
                    if (isHittingEnemy)
                    {
                        currentInteraction = new WaitForTimerToFinish();
                    }
                    time = 1;
                    isWalking = false;
                    moveIsBasedOnAnimation = false;
                    isFinished = true;
                }
            }

            transform.position = Vector3.Lerp(originPosition, targetPosition, time);

            return isFinished;
        }

        public void SetInteractionSliderStatus()
        {
            InteractionSlider.value -= 1;
            if (InteractionSlider.value == 0)
            {
                UiManager.instance.ResetInteractionSlider(this);
            }
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

        public void ActionIsDone()
        {
            currentInteractionInstance.interactionStack.actions[stackIndex].ActionDone(this);
        }
        void PathfindToInteractionHook()
        {
            GameManager.instance.HandleMovingAction(CurrentNode.worldPosition);
            //LoadInteractionFromStoredInteractionHook();
        }
        public void LoadInteractionFromHookAndStore(InteractionHook interactionHook)
        {
            currentInteractionHook = interactionHook;
            PathfindToInteractionHook();
        }
        void LoadInteractionFromStoredInteractionHook()
        {
            currentInteractionHook.LoadInteraction(this);
        }

        public void HandleUnitAttack(Interaction interaction, float deltaTime)
        {
            interaction.StartMethod(this, animationClip, actionAnimation);

            if (interaction.TickIsFinished(this, deltaTime))
            {
                interaction.OnEnd(this);
                currentInteraction = null;
            }
            
        }
        void HandleInteraction(Interaction interaction, float deltaTime)
        {
            //interaction.StartMethod(this);
            //if (interaction.TickIsFinished(this, deltaTime))
            //{
            //    interaction.OnEnd(this);
            //    currentInteraction = null;
            //}

        }
        public void AddOnInteractionStack(InteractionStack stack)
        {
            InteractionInstance ii = new InteractionInstance
            {
                interactionStack = stack,
                gridUnit = this
            };

            interactionInstances.Add(ii);

            UiManager.instance.CreateUiObjectForInteraction(ii);
        }
        public void RemoveInteraction(InteractionInstance instance)
        {
            if (interactionInstances.Contains(instance))
            {
                interactionInstances.Remove(instance);
            }
        }
        public void LoadInteraction(Interaction targetInteraction)
        {
            currentInteraction = targetInteraction;
        }
        public void LoadInteractionStack(InteractionInstance instance)
        {
            stackIndex = 0;
            currentInteractionInstance = instance;
            currentInteractionInstance.interactionStack.LoadAction(this, stackIndex);
        }
        public void StackIsComplete()
        {
            Debug.Log("Stack is complete!");
            currentEnemyTarget = null;
            currentInteraction = null;

            isHittingEnemy = false;

            //Ui presentation of action
            //if (currentInteractionInstance.uiObject != null)
            //{
            //    currentInteractionInstance.uiObject.SetToDestroy();
            //    if (InteractionButton.instance != null)
            //    {
            //        InteractionButton.instance.OnClick();
            //    }
            //}
        }
    }
}