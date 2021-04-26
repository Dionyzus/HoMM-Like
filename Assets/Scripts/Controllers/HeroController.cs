using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class HeroController : GridUnit
    {
        bool failedToLoadPath = false;

        public RenderTexture heroImage;

        private Slider interactionSlider;
        public Slider InteractionSlider { get => interactionSlider; set => interactionSlider = value; }

        private bool isInteractionPointBlank;
        public bool IsInteractionPointBlank { get => isInteractionPointBlank; set => isInteractionPointBlank = value; }

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
                    LoadActionFromInteractionContainer(interactionInstance);
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
            MoveToRequestedLocation(LoadInteractionFromInteractionHook);
        }
        public void MoveToRequestedLocation(OnPathReachCallback callback)
        {
            if (isInteractionPointBlank)
            {
                LoadInteractionFromInteractionHook();
                if (PathfinderMaster.instance.pathLine.positionCount > 0)
                {
                    PathfinderMaster.instance.pathLine.positionCount = 0;
                }
                isInteractionPointBlank = false;
                return;
            }

            PathfinderMaster.instance.RequestPathfinder(LoadPath, callback);
        }
        public void PreviewPathToNode(Node target, InteractionHook interactionHook = null)
        {
            if (interactionHook != null)
            {
                PathfinderMaster.instance.RequestPathAndPreview(CurrentNode,
                    GridManager.instance.GetNode(interactionHook.interactionPoint.position, gridIndex), this);
            }
            else
            {
                if (!target.IsWalkable())
                {
                    Debug.Log("Target node is not walkable");
                }

                PathfinderMaster.instance.RequestPathAndPreview(CurrentNode, target, this);
            }
        }
        public void LoadPath(List<Node> path, OnPathReachCallback callback)
        {
            if (path == null || path.Count == 0)
            {
                Debug.Log("Failed to load path, path is null.");

                failedToLoadPath = true;
                MovingToLocationCompleted();

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
        public override void LoadActionFromInteractionContainer(InteractionInstance instance)
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

        public override void InitializeMoveToInteractionContainer(Node targetNode)
        {
            InteractionInstance ii = new InteractionInstance
            {
                interactionContainer = moveToLocationContainer,
                gridUnit = this
            };

            interactionInstance = ii;

            UiManager.instance.CreateUiObjectForInteraction(ii);
        }
        public override void MoveToLocation()
        {
            MoveToRequestedLocation(
                    MovingToLocationCompleted);
        }
        public override void MovingToLocationCompleted()
        {
            if (isInteractionInitialized)
                isInteractionInitialized = false;

            if (failedToLoadPath)
            {
                Debug.Log("Do some animation, maybe interaction button a bit diff");
                failedToLoadPath = false;
            }

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