using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections;

namespace HOMM_BM
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager instance;

        //Used since not all animations have hit event
        public bool unitReceivedHitDebug;

        public UnitController currentUnit;
        InteractionHook interactionHook;

        //Until some AI logic is added.
        float debugTime;

        List<UnitController> unitsQueue = new List<UnitController>();
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

            unitsQueue = new List<UnitController>(battleUnits);
            currentUnit = unitsQueue.First();

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
                    FlowmapPathfinderMaster.instance.ClearPathData();
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
                    if (debugTime >= 2.5f)
                    {
                        OnMoveFinished();
                        debugTime = 0;
                        return;
                    }
                    debugTime += Time.deltaTime;
                    return;
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
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //For easier hook detection, due to camera raycasting position
                    Vector3 offSet = new Vector3(0.35f, 0, 0.35f);

                    interactionHook = CheckForInteractionHook(hit.point + offSet);

                    Node targetNode = GridManager.instance.GetNode(hit.point - offSet, currentUnit.gridIndex);

                    if (interactionHook != null)
                    {
                        if (isTargetPointBlank)
                            isTargetPointBlank = false;

                        if (!unitIsMoving)
                        {
                            if (FlowmapPathfinderMaster.instance.IsTargetNodeNeighbour(currentUnit.CurrentNode, targetNode))
                            {
                                FlowmapPathfinderMaster.instance.ClearPathData();
                                isTargetPointBlank = true;
                            }
                        }
                        if ((interactionHook.interactionContainer != null && FlowmapPathfinderMaster.instance.previousPath.Count != 0) || isTargetPointBlank)
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                currentUnit.currentInteractionHook = interactionHook;
                                currentUnit.IsInteractionInitialized = true;
                                currentUnit.CreateInteractionContainer(interactionHook.interactionContainer);

                                UnitController targetUnit = interactionHook.GetComponentInParent<UnitController>();

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
                            }
                        }
                    }

                    if (interactionHook == null && !FlowmapPathfinderMaster.instance.reachableNodes.Contains(targetNode))
                    {
                        FlowmapPathfinderMaster.instance.ClearPathData();
                    }
                }

                if (interactionHook == null && currentMouseLogic != null)
                    currentMouseLogic.InteractTick(this, hit);
            }
        }

        public InteractionHook CheckForInteractionHook(Vector3 center)
        {
            Collider[] hitColliders = Physics.OverlapSphere(center, 0.75f, 9);
            foreach (Collider hitCollider in hitColliders)
            {
                InteractionHook ih = hitCollider.transform.GetComponentInParent<InteractionHook>();
                if (ih != null)
                    return ih;
            }
            return null;
        }

        //Something which could possibly be used
        public bool IsInteractionHookInRadius(Node originNode, Node targetNode)
        {
            float xDistance = Mathf.Abs(originNode.position.x - targetNode.position.x);
            float zDistance = Mathf.Abs(originNode.position.z - targetNode.position.z);

            if (xDistance >= 1 && zDistance >= 1)
            {
                return false;
            }
            return true;
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

            unitsQueue.RemoveAt(0);
            unitsQueue.Add(currentUnit);

            previousUnit = currentUnit;

            currentUnit = unitsQueue.First();

            OnCurrentUnitTurn();
        }

        public void UnitDeathCallback(UnitController unitController)
        {
            unitsQueue.Remove(unitController);
            UiManager.instance.UpdateUiOnUnitDeath(unitController);
        }
    }
}