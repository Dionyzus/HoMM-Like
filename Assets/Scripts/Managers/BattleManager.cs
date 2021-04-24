using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections;

namespace HOMM_BM
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager instance;

        //Used since not all animations have hit event
        public bool unitReceivedHitDebug;

        public UnitController currentUnit;

        //Until some AI logic is added.
        float debugTime;

        Queue<UnitController> unitsQueue;
        UnitController[] battleUnits;

        bool isTargetPointBlank;
        public bool calculatePath;

        [HideInInspector]
        public bool unitIsMoving;

        private CombatEvent currentCombatEvent;
        public CombatEvent CurrentCombatEvent { get => currentCombatEvent; set => currentCombatEvent = value; }

        MouseLogicBattle currentMouseLogic;
        public MouseLogicBattle selectMove;
        public MouseLogicBattle targetNodeAction;

        UnitController previousUnit;
        public UnitController PreviousUnit { get => previousUnit; set => previousUnit = value; }

        private void Awake()
        {
            instance = this;
        }
        public void Initialize()
        {
            battleUnits = FindObjectsOfType<UnitController>();
            Array.Sort(battleUnits,
                    delegate (UnitController unitA, UnitController unitB) { return unitB.Initiative.CompareTo(unitA.Initiative); });

            unitsQueue = new Queue<UnitController>(battleUnits);
            currentUnit = unitsQueue.Peek();

            //True as in initializing
            OnCurrentUnitTurn(true);
        }

        private void Update()
        {
            if (!GameManager.instance.CurrentGameState.Equals(GameManager.GameState.BATTLE))
                return;

            if (unitIsMoving)
            {
                if (currentUnit.MovingOnPathFinished())
                {
                    FlowmapPathfinderMaster.instance.pathLine.positionCount = 0;
                    FlowmapPathfinderMaster.instance.previousPath.Clear();
                    unitIsMoving = false;

                    if (!currentUnit.IsInteractionInitialized)
                        OnMoveFinished();
                }
            }

            else
            {
                //Atm just for debuging purposes
                if (currentUnit.gameObject.layer == GridManager.ENEMY_UNITS_LAYER)
                {
                    if (debugTime >= 2)
                    {
                        OnMoveFinished();
                        debugTime = 0;
                        return;
                    }
                    debugTime += Time.deltaTime;
                    return;
                }

                if (currentUnit != null)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out RaycastHit hit, 100))
                    {
                        //Need to add check for range
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
                                    if (FlowmapPathfinderMaster.instance.IsTargetNodeNeighbour(currentUnit.CurrentNode, targetNode))
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

                                        UnitController targetUnit = hook.GetComponentInParent<UnitController>();

                                        if (targetUnit != null)
                                        {
                                            currentCombatEvent = new CombatEvent(currentUnit, targetUnit);
                                        }

                                        if (FlowmapPathfinderMaster.instance.previousPath.Count > 0)
                                        {
                                            HandleMovingAction(currentUnit.CurrentNode.worldPosition);
                                        }

                                        if (isTargetPointBlank)
                                        {
                                            currentUnit.IsTargetPointBlank = true;
                                            isTargetPointBlank = false;
                                            FlowmapPathfinderMaster.instance.ClearFlowmapData();
                                        }

                                        return;
                                    }
                                }
                            }
                        }
                    }
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
            if (currentUnit.IsInteractionInitialized || currentUnit.IsInteracting || currentUnit.currentInteractionHook != null)
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                if (currentMouseLogic != null)
                    currentMouseLogic.InteractTick(this, hit);
            }
        }

        public void OnCurrentUnitTurn(bool isInitialize = false)
        {
            if (currentUnit != null)
            {
                UiManager.instance.OnUnitTurn(unitsQueue, currentUnit, isInitialize);
                currentMouseLogic = selectMove;
            }
            else
            {
                Debug.Log("Something went wrong, current unit is null!");
            }
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
        public void OnMoveFinished()
        {
            unitReceivedHitDebug = false;

            unitsQueue.Dequeue();
            unitsQueue.Enqueue(currentUnit);

            previousUnit = currentUnit;

            currentUnit = unitsQueue.Peek();

            OnCurrentUnitTurn();
        }

        public void UnitDeathCallback(UnitController unitController)
        {
            //Remove from existing queue etc. etc.
        }
    }
}