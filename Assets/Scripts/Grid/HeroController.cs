using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class HeroController : GridUnit
    {
        private Slider interactionSlider;
        public Slider InteractionSlider { get => interactionSlider; set => interactionSlider = value; }

        //private void Start()
        //{
        //    stepsCountSlider = stepsCount;
        //}

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            //Check this out...
            //animator.applyRootMotion = IsInteracting;
            animator.SetBool("isWalking", isWalking);

            if (currentPath.Count > 0)
            {
                HandleMovement(deltaTime);
            }
            else
            {
                isWalking = false;
            }

            if (isInteractionInitialized)
            {
                if (interactionInstance != null)
                {
                    LoadIntoInteractionContainer(interactionInstance);
                    interactionInstance = null;
                }
            }

            if (currentInteraction != null)
            {
                HandleInteraction(currentInteraction, deltaTime);
            }
        }

        public void SetInteractionSliderStatus()
        {
            InteractionSlider.value -= 1;
            if (InteractionSlider.value == 0)
            {
                UiManager.instance.ResetInteractionSlider(this);
            }
        }

        void HandleMovement(float deltaTime)
        {
            isWalking = false;

            if (time > 0)
            {
                time -= deltaTime;
                return;
            }

            if (!isPathInitialized)
            {
                originPosition = CurrentNode.worldPosition;
                targetPosition = currentPath[index].worldPosition;
                float distance = Vector3.Distance(originPosition, targetPosition);
                actualMovementSpeed = distance / movementSpeed;

                targetRotation = Quaternion.LookRotation((targetPosition - originPosition).normalized);
                moveTime = 0;

                isPathInitialized = true;
            }

            bool pathIsFinished = false;

            isWalking = true;

            moveTime += deltaTime / actualMovementSpeed;
            rotationTime += deltaTime / rotationSpeed;

            if (rotationTime > 1)
            {
                rotationTime = 1;
            }

            if (moveTime > 1)
            {
                moveTime = 1;
                isPathInitialized = false;
                index++;

                PathfinderMaster.instance.pathLine.positionCount -= 1;
                SetInteractionSliderStatus();

                if (index > currentPath.Count - 1)
                {
                    pathIsFinished = true;
                }
            }

            Vector3 tp = Vector3.Lerp(originPosition, targetPosition, moveTime);
            transform.position = tp;

            Vector3 direction = (targetPosition - originPosition).normalized;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime / rotationSpeed);

            if (pathIsFinished)
            {
                currentPath.Clear();
                onPathReachCallback?.Invoke();
            }
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
            PathfindToInteractionHook();
        }
        void PathfindToInteractionHook()
        {
            RequestPathfindToNode(GridManager.instance.GetNode(currentInteractionHook.interactionPoint.position, gridIndex),
                    LoadInteractionFromInteractionHook);
        }
        void RequestPathfindToNode(Node target, OnPathReachCallback callback)
        {
            //This could be problem..
            if (!target.isWalkable)
            {
                callback?.Invoke();
            }

            PathfinderMaster.instance.RequestPathfinder(CurrentNode, target, LoadPath, callback, this);
        }
        public void LoadPath(List<Node> path, OnPathReachCallback callback)
        {
            if (path == null || path.Count == 0)
            {
                Debug.Log("Failed to load path, path is null.");
                return;
            }
            else
            {
                currentPath = path;
                index = 0;
                isPathInitialized = false;
                onPathReachCallback = callback;
            }
        }
        void LoadInteractionFromInteractionHook()
        {
            currentInteractionHook.LoadInteraction(this);
            isInteractionInitialized = false;
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

            Destroy(currentInteractionHook.gameObject);
            currentInteractionHook = null;

            if (currentInteractionInstance.uiObject != null)
            {
                currentInteractionInstance.uiObject.SetToDestroy();
                if (InteractionButton.instance != null)
                {
                    InteractionButton.instance.OnClick();
                }
            }
        }
    }
}