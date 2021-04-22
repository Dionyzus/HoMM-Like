using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HOMM_BM
{
    public class BattleManager : GameManager
    {
        public UnitController currentUnit;
        UnitController storedUnit;

        bool isTargetPointBlank;

        public bool calculatePath;

        [HideInInspector]
        public bool unitIsMoving;

        MouseLogicBattle currentMouseLogic;
        public MouseLogicBattle selectMove;
        public MouseLogicBattle targetNodeAction;

        private void Awake()
        {
            BattleManager = this;
        }
        private void Update()
        {
            if (!instance.CurrentGameState.Equals(GameState.BATTLE))
                return;

            if (storedUnit != null)
            {
                if (storedUnit.IsInteracting)
                    return;
                storedUnit = null;
            }

            if (unitIsMoving)
            {
                if (currentUnit.MovingOnPathFinished())
                {
                    FlowmapPathfinderMaster.instance.pathLine.positionCount = 0;
                    FlowmapPathfinderMaster.instance.previousPath.Clear();
                    unitIsMoving = false;

                    if (!currentUnit.IsInteractionInitialized)
                        calculatePath = true;
                }
            }

            else
            {
                if (currentUnit != null)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out RaycastHit hit, 100))
                    {
                        if (EventSystem.current.IsPointerOverGameObject())
                        {
                            InteractionHook hook = hit.transform.GetComponentInParent<InteractionHook>();

                            if (hook != null)
                            {
                                if (isTargetPointBlank)
                                    isTargetPointBlank = false;

                                if (!unitIsMoving)
                                {
                                    Node targetNode = GridManager.instance.GetNode(hit.point, currentUnit.gridIndex);
                                    if (FlowmapPathfinderMaster.instance.IsCurrentNodeNeighbour(currentUnit.CurrentNode, targetNode))
                                    {
                                        FlowmapPathfinderMaster.instance.pathLine.positionCount = 0;
                                        FlowmapPathfinderMaster.instance.previousPath.Clear();
                                        FlowmapPathfinderMaster.instance.ClearHighlightedNodes();

                                        isTargetPointBlank = true;
                                    }
                                }
                                if ((hook.interactionContainer != null && FlowmapPathfinderMaster.instance.previousPath.Count != 0) || isTargetPointBlank)
                                {
                                    if (Input.GetMouseButtonDown(0))
                                    {
                                        currentUnit.currentInteractionHook = hook;
                                        currentUnit.IsInteractionInitialized = true;
                                        currentUnit.CreateInteractionContainer(hook.interactionContainer);

                                        if (FlowmapPathfinderMaster.instance.previousPath.Count > 0)
                                        {
                                            HandleMovingAction(currentUnit.CurrentNode.worldPosition);
                                        }

                                        if (isTargetPointBlank)
                                        {
                                            currentUnit.isTargetPointBlank = true;
                                            isTargetPointBlank = false;
                                        }

                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (currentMouseLogic == null)
                        currentMouseLogic = selectMove;
                }

                HandleMouse();
            }

            if (calculatePath)
            {
                FlowmapPathfinderMaster.instance.CalculateWalkablePositions();
                calculatePath = false;
            }
        }

        void HandleMouse()
        {
            if (currentUnit != null && (currentUnit.IsInteracting || currentUnit.currentInteractionHook != null))
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                if (currentMouseLogic != null)
                    currentMouseLogic.InteractTick(this, hit);
            }
        }

        public void OnCurrentUnitTurn(UnitController gridUnit)
        {
            if (unitIsMoving)
                return;

            if (currentUnit == gridUnit)
                return;

            if (gridUnit != null)
            {
                currentUnit = gridUnit;
                currentMouseLogic = selectMove;

                UiManager.instance.OnUnitTurn(currentUnit);

                if (currentMouseLogic != null)
                    currentMouseLogic.InteractTick(this, currentUnit);
            }
        }
        public void UnitDeath(UnitController gridUnit)
        {
            if (currentUnit == gridUnit)
                currentUnit = null;

            storedUnit = gridUnit;
            calculatePath = true;
        }
        public void HandleMovingAction(Vector3 origin)
        {
            if (currentUnit != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    FlowmapPathfinderMaster.instance.InitiateMovingOnPath(currentUnit);
                }

                FlowmapPathfinderMaster.instance.CalculateNewPath(origin, currentUnit);
            }
        }
    }
}