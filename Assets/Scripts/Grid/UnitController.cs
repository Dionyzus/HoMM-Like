using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class UnitController : GridUnit
    {
        public bool isUnitDeadDebug;
        public bool isTargetPointBlank;

        private void Update()
        {
            float deltaTime = Time.deltaTime;

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

            if (isInteractionInitialized && isTargetPointBlank)
            {
                if (interactionInstance != null)
                {
                    LoadIntoInteractionContainer(interactionInstance);
                    interactionInstance = null;
                }
                currentInteractionHook.LoadInteraction(this);
                isInteractionInitialized = false;
            }

            if (isInteractionInitialized && !isTargetPointBlank)
            {
                if (interactionInstance != null)
                {
                    LoadIntoInteractionContainer(interactionInstance);
                    interactionInstance = null;
                }
                TryLoadingInteractionFromHook();
            }

            if (currentInteraction != null)
            {
                HandleInteraction(currentInteraction, deltaTime);
            }
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
            GameManager.BattleManager.UnitDeath(this);
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
                FlowmapPathfinderMaster.instance.pathLine.positionCount -= 1;

                if (index > currentPath.Count - 1)
                {
                    time = 1;
                    isWalking = false;
                    moveIsBasedOnAnimation = false;
                    isFinished = true;
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
        public override void CreateInteractionContainer(InteractionContainer container)
        {
            InteractionInstance ii = new InteractionInstance
            {
                interactionContainer = container,
                gridUnit = this
            };
            interactionInstance = ii;

            UiManager.instance.CreateUiObjectForInteraction(ii);
        }
        public override void StoreInteractionHook(InteractionHook interactionHook)
        {
            currentInteractionHook = interactionHook;
        }
        void TryLoadingInteractionFromHook()
        {
            if (!GameManager.BattleManager.unitIsMoving)
            {
                currentInteractionHook.LoadInteraction(this);
                isInteractionInitialized = false;
            }
        }
        public override void LoadInteraction(Interaction targetInteraction)
        {
            currentInteraction = targetInteraction;
        }
        protected override void HandleInteraction(Interaction interaction, float deltaTime)
        {
            interaction.StartMethod(this);
            if (interaction.TickIsFinished(this, deltaTime))
            {
                interaction.OnEnd(this);
                currentInteraction = null;
            }
        }
        public override void ActionIsDone()
        {
            currentInteractionInstance.interactionContainer.action.ActionDone(this);
        }
        public override void LoadIntoInteractionContainer(InteractionInstance instance)
        {
            currentInteractionInstance = instance;
            currentInteractionInstance.interactionContainer.LoadAction(this);
        }
        public override void InteractionCompleted()
        {
            currentInteraction = null;
            currentInteractionHook = null;

            //If unit losses all hp, kill unit, leave dead body and now it is walkable
            //Keep "interaction", since we wanna keep track of all the moves

            //Keep this until adding select next unit
            GameManager.BattleManager.calculatePath = true;
        }
    }
}