using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using System.Collections;
using Cinemachine;

namespace HOMM_BM
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager instance;
        public Camera MainCamera;

        bool preparationStateFinished;

        public Material tacticalNodesMaterial;
        public GridPosition startGridPosition;

        List<Node> tacticalNodes = new List<Node>();
        List<Node> normalizedTacticalNodes = new List<Node>();

        [SerializeField]
        CinemachineVirtualCamera battleCamera = default;
        [SerializeField]
        CinemachineVirtualCamera preparationCamera = default;
        [SerializeField]
        CinemachineVirtualCamera combatCamera = default;

        //Used since not all animations have hit event
        public bool unitReceivedHitDebug;

        public UnitController currentUnit;
        InteractionHook interactionHook;

        //Until some AI logic is added.
        float AIDebugTime;

        float preparationDebugTime;

        List<UnitController> unitsQueue = new List<UnitController>();
        UnitController[] battleUnits;

        bool isTargetPointBlank;
        private bool calculatePath;

        [HideInInspector]
        public bool unitIsMoving;

        private CombatEvent currentCombatEvent;
        public CombatEvent CurrentCombatEvent { get => currentCombatEvent; set => currentCombatEvent = value; }

        MouseLogicBattle currentMouseLogic;
        public MouseLogicBattle selectMove;
        public MouseLogicBattle targetNodeAction;

        UnitController previousUnit;
        public UnitController PreviousUnit { get => previousUnit; set => previousUnit = value; }
        public bool CalculatePath { get => calculatePath; set => calculatePath = value; }

        private void Awake()
        {
            instance = this;
        }
        public void Initialize()
        {
            battleUnits = FindObjectsOfType<UnitController>();
            Array.Sort(battleUnits,
                    delegate (UnitController unitA, UnitController unitB) { return unitB.Initiative.CompareTo(unitA.Initiative); });

            ActivatePreparationCamera();
            CreateTacticalNodes();

            unitsQueue = new List<UnitController>(battleUnits);
            currentUnit = unitsQueue.First();

            //True as in initializing
            OnCurrentUnitTurn(true);

        }

        private void Update()
        {
            if (!GameManager.instance.CurrentGameState.Equals(GameManager.GameState.BATTLE))
                return;

            if (!preparationStateFinished)
                preparationDebugTime += Time.deltaTime;

            if (!preparationStateFinished && (preparationDebugTime >= 10f || GameManager.instance.Keyboard.escapeKey.isPressed))
            {
                preparationStateFinished = true;

                DisableDragNDropComponent();
                ClearTacticalNodes();

                DeactivatePreparationCamera();
                ActivateBattleCamera();
            }

            if (!preparationStateFinished)
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
                    if (AIDebugTime >= 2.5f)
                    {
                        OnMoveFinished();
                        AIDebugTime = 0;
                        return;
                    }
                    AIDebugTime += Time.deltaTime;
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

        //Later on need to add walkable check
        void CreateTacticalNodes()
        {
            Vector3 nodeScale = (Vector3.one * 0.95f) * 1;
            int xRange = Mathf.FloorToInt(Mathf.Abs(GridManager.instance.GridPositions[0].transform.position.x -
                GridManager.instance.GridPositions[1].transform.position.x));

            Vector3 initialPosition = startGridPosition.transform.position;

            //Probably there should be some better way to get this
            List<UnitController> friendlyUnits = new List<UnitController>();
            foreach (UnitController unit in battleUnits)
            {
                if (unit.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                {
                    friendlyUnits.Add(unit);
                }
            }

            //Later on add something cool with unit positions if wanted

            foreach (UnitController unit in friendlyUnits)
            {
                Node initialNode = GridManager.instance.GetNode(initialPosition, unit.gridIndex);

                if (initialNode != null)
                {
                    Vector3 targetPosition = initialNode.worldPosition;
                    targetPosition.y += 1.01f;
                    unit.transform.position = targetPosition;

                    xRange -= 1;
                    initialPosition.x += 1;
                }
                else
                {
                    Debug.Log("Node is null, something went wrong!");
                }
            }

            for (int x = 0; x <= xRange; x++)
            {
                Node node = GridManager.instance.GetNode(initialPosition, 0);
                tacticalNodes.Add(node);

                Vector3 normalizedPosition = new Vector3(Mathf.Round(initialPosition.x), 1, Mathf.Round(initialPosition.z));
                Node normalizedNode = GridManager.instance.GetNode(normalizedPosition, 0);
                if (normalizedNode != null)
                    normalizedTacticalNodes.Add(normalizedNode);

                if (node != null)
                {
                    CreateReferenceForNode(nodeScale, node);
                }

                initialPosition.x += 1;
            }

            //Magic setup, after setting up unit positions, maybe find some other way
            xRange += friendlyUnits.Count;
            initialPosition.z += 1;

            for (int x = -1; x <= xRange; x++)
            {
                Node node = GridManager.instance.GetNode(initialPosition, 0);
                tacticalNodes.Add(node);

                Vector3 normalizedPosition = new Vector3(Mathf.Round(initialPosition.x), 1, Mathf.Round(initialPosition.z));
                Node normalizedNode = GridManager.instance.GetNode(normalizedPosition, 0);
                if (normalizedNode != null)
                    normalizedTacticalNodes.Add(normalizedNode);

                if (node != null)
                {
                    CreateReferenceForNode(nodeScale, node);
                }
                initialPosition.x -= 1;
            }
        }

        public List<Node> GetNormalizedTacticalNodes()
        {
            return normalizedTacticalNodes;
        }

        private void CreateReferenceForNode(Vector3 nodeScale, Node node, bool unitNode = false)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(go.GetComponent<Collider>());
            Vector3 targetPosition = node.worldPosition;
            targetPosition.y += 1.01f;
            go.transform.position = targetPosition;
            go.transform.eulerAngles = new Vector3(90, 0, 0);
            go.transform.parent = GridManager.instance.GridParents[0].transform;
            go.transform.localScale = nodeScale;

            node.renderer = go.GetComponentInChildren<Renderer>();
            if (!unitNode)
                node.renderer.material = tacticalNodesMaterial;
        }

        void ClearTacticalNodes()
        {
            foreach (Node node in tacticalNodes)
            {
                if (node != null)
                    GridManager.instance.ClearNode(node);
            }
            tacticalNodes.Clear();

            foreach (Node node in normalizedTacticalNodes)
            {
                if (node != null)
                    GridManager.instance.ClearNode(node);
            }
            normalizedTacticalNodes.Clear();

            GridManager.instance.ClearGrids();
        }

        void DisableDragNDropComponent()
        {
            DragNDropUnit[] draggableUnits = GetComponentsInChildren<DragNDropUnit>();

            foreach (DragNDropUnit unit in draggableUnits)
            {
                unit.enabled = false;
            }
        }

        void HandleMouse()
        {
            if (currentUnit.IsInteractionInitialized || currentUnit.IsInteracting || currentUnit.currentInteractionHook != null)
                return;

            Ray ray = MainCamera.ScreenPointToRay(GameManager.instance.Mouse.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, 100))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    //For easier hook detection, due to camera raycasting position
                    Vector3 offSet = new Vector3(0.3f, 0, 0.3f);

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
                            if (GameManager.instance.Mouse.leftButton.isPressed)
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
            Collider[] hitColliders = Physics.OverlapSphere(center, 0.8f, 9);
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

        public float xOffsetCombatCamera = 10;
        public float zOffsetCombatCamera = -10;
        public float yOffsetCombatCamera;
        public void ActivateCombatCamera(Transform target)
        {
            combatCamera.Priority = 50;

            float xPosition = target.position.x + xOffsetCombatCamera;
            float zPosition = target.position.x + zOffsetCombatCamera;
            float yPosition = target.position.x + yOffsetCombatCamera;

            Vector3 targetPosition = new Vector3(xPosition, yPosition, zPosition);

            combatCamera.transform.position = targetPosition;
            combatCamera.LookAt = target;
        }
        public void DeactivateCombatCamera()
        {
            combatCamera.Priority = 10;
        }
        public void ActivatePreparationCamera()
        {
            preparationCamera.Priority = 50;
        }
        public void DeactivatePreparationCamera()
        {
            preparationCamera.Priority = 10;
        }
        public void ActivateBattleCamera()
        {
            battleCamera.Priority = 50;
        }
        public void DeactivateBattleCamera()
        {
            battleCamera.Priority = 10;
        }

        public void HandleMovingAction(Vector3 origin)
        {
            if (currentUnit != null)
            {
                if (GameManager.instance.Mouse.leftButton.isPressed)
                {
                    FlowmapPathfinderMaster.instance.InitiateMovingOnPath(currentUnit);
                }

                FlowmapPathfinderMaster.instance.CalculateNewPath(origin, currentUnit);
            }
        }
        public void OnMoveFinished()
        {
            DeactivateCombatCamera();
            ActivateBattleCamera();

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