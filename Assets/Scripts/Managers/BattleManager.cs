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

        //Will be enums
        bool preparationStateFinished;
        bool battleStateStarted;
        bool cleanUpStateFinished;

        public Material tacticalNodesMaterial;

        public GridPosition startGridPosition;
        public GridPosition endGridPosition;

        //Probably there should be some better way to get this
        [HideInInspector]
        public List<UnitController> FriendlyUnits = new List<UnitController>();
        [HideInInspector]
        public List<UnitController> EnemyUnits = new List<UnitController>();

        List<Node> tacticalNodes = new List<Node>();
        List<Node> normalizedTacticalNodes = new List<Node>();

        [HideInInspector]
        public List<Node> InvalidNodes = new List<Node>();

        Vector3 enemyInitialPosition;

        //I guess there could a better way to solve this, maybe 
        //change walkability as soon as unit lands on Node, atm
        //just dictionary of friendly unit nodes
        Dictionary<UnitController, Node> unitsNodes = new Dictionary<UnitController, Node>();

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

        float waitForCleanupTime;
        float waitForNewTurn;

        private void Awake()
        {
            instance = this;
        }
        public void Initialize()
        {
            battleUnits = FindObjectsOfType<UnitController>();
            Array.Sort(battleUnits,
                    delegate (UnitController unitA, UnitController unitB) { return unitB.Initiative.CompareTo(unitA.Initiative); });

            //Probably swap this two once dynamic world to battle is implemented
            ActivatePreparationCamera();
            InitializePreparationPhase();
            //Creating available tactical positions to move onto
            CreateTacticalNodesForSmallUnits();
            CreateTacticalNodesForBigUnits();

            unitsQueue = new List<UnitController>(battleUnits);
            currentUnit = unitsQueue.First();

            //True as in initializing
            OnCurrentUnitTurn(true);
        }

        private void Update()
        {
            if (!GameManager.instance.CurrentGameState.Equals(Enums.GameState.BATTLE))
                return;

            if (!preparationStateFinished)
                preparationDebugTime += Time.deltaTime;

            if (!preparationStateFinished && (preparationDebugTime >= 30f || GameManager.instance.Keyboard.escapeKey.isPressed))
            {
                preparationStateFinished = true;

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                //Clearing
                DisableDragNDropComponent();
                ClearInvalidNodes();
                ClearTacticalNodes();

                return;
            }

            if (preparationStateFinished && !cleanUpStateFinished)
            {
                //Initializing enemies
                InitializeEnemySide();

                //Initialize camera look up
                DeactivatePreparationCamera();
                ActivateBattleCamera();

                cleanUpStateFinished = true;

                return;
            }

            if (cleanUpStateFinished && !battleStateStarted)
            {
                waitForCleanupTime += Time.deltaTime;

                if (waitForCleanupTime <= 1.5f)
                {
                    return;
                }

                battleStateStarted = true;

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                EnemyUnitManager.instance.Initialize();

                UiManager.instance.ActivateBattleUi();
            }

            if (!battleStateStarted)
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
                if (currentUnit.gameObject.layer == GridManager.ENEMY_UNITS_LAYER)
                {
                    waitForNewTurn += Time.deltaTime;

                    //This timer could be avoided if all attacks have animation hit event
                    if (waitForNewTurn >= 1)
                    {
                        waitForNewTurn = 0;

                        if (EnemyUnitManager.instance.AiInteracting || currentUnit.IsInteractionInitialized)
                            return;

                        FlowmapPathfinderMaster.instance.CalculateWalkablePositions();
                        calculatePath = false;

                        EnemyUnitManager.instance.HandleAiTurn(currentUnit);
                        return;
                    }
                }
                else
                {
                    HandleMouse();
                }
            }

            if (calculatePath)
            {
                FlowmapPathfinderMaster.instance.CalculateWalkablePositions();
                calculatePath = false;
            }
        }

        //Later on need to add walkable check
        void InitializePreparationPhase()
        {
            int xGridRange = Mathf.FloorToInt(Mathf.Abs(startGridPosition.transform.position.x -
                endGridPosition.transform.position.x));

            Vector3 friendlyInitialPosition = startGridPosition.transform.position;

            InitializeUnitLists();
            InitializeFriendlySide(xGridRange, friendlyInitialPosition);
        }
        //We wont know number of units once battle actually starts
        //Atm just reading existing units in the scene
        //Add instantiating from prefab
        private void InitializeUnitLists()
        {
            foreach (UnitController unit in battleUnits)
            {
                if (unit.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                {
                    FriendlyUnits.Add(unit);
                }
                else
                {
                    EnemyUnits.Add(unit);
                    unit.gameObject.SetActive(false);
                }
            }
        }

        int xOffsetGridIndexZero = 1;
        int xOffsetGridIndexOne = 2;
        private void InitializeFriendlySide(int xGridRange, Vector3 friendlyInitialPosition)
        {
            //Later on add something cool with unit positions if wanted
            foreach (UnitController unit in FriendlyUnits)
            {
                Node initialNode = GridManager.instance.GetNode(friendlyInitialPosition, unit.GridIndex);

                if (initialNode != null)
                {
                    Vector3 targetPosition = initialNode.worldPosition;
                    targetPosition.y += 1.01f;

                    unit.transform.position = targetPosition;

                    //At some point add node render chaning depending on placed/tactical,
                    //picked one should be standard, then previous position becomes tactical etc.
                    CreateReferenceForNode(initialNode, unit.GridIndex + 1);
                    tacticalNodes.Add(initialNode);

                    Node normalizedNode = CreateNormalizedNode(friendlyInitialPosition, unit.GridIndex);
                    if (normalizedNode != null)
                        normalizedTacticalNodes.Add(normalizedNode);

                    unitsNodes.Add(unit, normalizedNode);

                    //Adding to x side of grids as units are being placed
                    if (unit.GridIndex == 0)
                    {
                        xGridRange -= xOffsetGridIndexZero;
                        friendlyInitialPosition.x += xOffsetGridIndexZero;
                    }
                    else
                    {
                        xGridRange -= xOffsetGridIndexOne;
                        friendlyInitialPosition.x += xOffsetGridIndexOne;
                    }
                }
                else
                {
                    Debug.Log("Node is null, something went wrong!");
                }
            }
        }

        //Maybe try find different way to check if position is correct
        public Node CreateNormalizedNode(Vector3 position, int gridIndex)
        {
            Vector3 normalizedPosition;
            if (gridIndex == 0)
            {
                normalizedPosition = new Vector3(Mathf.Round(position.x), 0, Mathf.Round(position.z));
                return GridManager.instance.GetNode(normalizedPosition, gridIndex);
            }
            normalizedPosition = new Vector3(Mathf.FloorToInt(position.x), 0, Mathf.Round(position.z));
            return GridManager.instance.GetNode(normalizedPosition, gridIndex);
        }

        //Probably will need to have some standard grid sizes to avoid eventual problems with calculations
        //Even tho having properly placed grid positions should ensure there are no unplanned issues
        private void CreateTacticalNodesForSmallUnits()
        {
            int xGridRange = Mathf.FloorToInt(Mathf.Abs(startGridPosition.transform.position.x -
                endGridPosition.transform.position.x));
            Vector3 friendlyInitialPosition = startGridPosition.transform.position;

            for (int x = 0; x < xGridRange; x++)
            {
                Node node = GridManager.instance.GetNode(friendlyInitialPosition, 0);
                tacticalNodes.Add(node);

                Node normalizedNode = CreateNormalizedNode(friendlyInitialPosition, 0);
                if (normalizedNode != null)
                    normalizedTacticalNodes.Add(normalizedNode);

                if (node != null && !unitsNodes.ContainsValue(normalizedNode))
                {
                    CreateReferenceForNode(node, xOffsetGridIndexZero);
                }
                friendlyInitialPosition.x += xOffsetGridIndexZero;
            }

            //Second row, starts from last x node -1, since we incremented x 1 additional time
            //in first row
            friendlyInitialPosition.x -= xOffsetGridIndexZero;
            friendlyInitialPosition.z += xOffsetGridIndexZero;

            for (int x = 0; x < xGridRange; x++)
            {
                Node node = GridManager.instance.GetNode(friendlyInitialPosition, 0);
                tacticalNodes.Add(node);

                Node normalizedNode = CreateNormalizedNode(friendlyInitialPosition, 0);
                if (normalizedNode != null)
                    normalizedTacticalNodes.Add(normalizedNode);

                if (node != null && !unitsNodes.ContainsValue(normalizedNode))
                {
                    CreateReferenceForNode(node, xOffsetGridIndexZero);
                }
                friendlyInitialPosition.x -= xOffsetGridIndexZero;
            }
        }

        private void CreateTacticalNodesForBigUnits()
        {
            int xGridRange = Mathf.FloorToInt(Mathf.Abs(startGridPosition.transform.position.x -
                endGridPosition.transform.position.x));
            Vector3 friendlyInitialPosition = startGridPosition.transform.position;

            for (int x = 0; x < xGridRange; x += xOffsetGridIndexOne)
            {
                Node node = GridManager.instance.GetNode(friendlyInitialPosition, 1);
                tacticalNodes.Add(node);

                Node normalizedNode = CreateNormalizedNode(friendlyInitialPosition, 1);
                if (normalizedNode != null)
                    normalizedTacticalNodes.Add(normalizedNode);

                if (node != null && !unitsNodes.ContainsValue(normalizedNode))
                {
                    CreateReferenceForNode(node, xOffsetGridIndexOne);
                }
                friendlyInitialPosition.x += xOffsetGridIndexOne;
            }
        }

        private void InitializeEnemySide()
        {
            //Later on add something cool with unit positions if wanted
            //Atm just a little bit of an offset
            enemyInitialPosition = endGridPosition.transform.position;
            //Starting from "zero" position, need to add one to z coord
            //Positions available in row 1 and 2
            enemyInitialPosition.z -= 1;

            //Atm using magic number 5 just to add a bit of spread to units
            foreach (UnitController unit in EnemyUnits)
            {
                Vector3 offsetPosition;

                if (unit.GridIndex == 1)
                {
                    offsetPosition = new Vector3(enemyInitialPosition.x - 5, enemyInitialPosition.y, enemyInitialPosition.z - 1);
                }
                else
                {
                    offsetPosition = new Vector3(enemyInitialPosition.x - 5, enemyInitialPosition.y, enemyInitialPosition.z - 1);
                }

                Node initialNode = GridManager.instance.GetNode(offsetPosition, unit.GridIndex);
                if (initialNode != null)
                {
                    unit.transform.position = initialNode.worldPosition;
                    unit.gameObject.SetActive(true);

                    enemyInitialPosition.x -= 5;
                }
                else
                {
                    Debug.Log("Node is null, something went wrong!");
                }
            }
        }
        private void CreateReferenceForNode(Node node, int nodeScaleMultiplier)
        {
            Vector3 nodeScale = (Vector3.one * 0.95f) * nodeScaleMultiplier;
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(go.GetComponent<Collider>());
            Vector3 targetPosition = node.worldPosition;
            targetPosition.y += 1.01f;
            go.transform.position = targetPosition;
            go.transform.eulerAngles = new Vector3(90, 0, 0);
            go.transform.parent = GridManager.instance.GridParents[0].transform;
            go.transform.localScale = nodeScale;

            node.renderer = go.GetComponentInChildren<Renderer>();
            node.renderer.material = tacticalNodesMaterial;
        }

        public List<Node> GetNormalizedTacticalNodes()
        {
            return normalizedTacticalNodes;
        }
        public Dictionary<UnitController, Node> GetUnitsNodes()
        {
            return unitsNodes;
        }

        void ClearTacticalNodes()
        {
            foreach (Node node in tacticalNodes)
            {
                foreach (Node subNode in node.subNodes)
                {
                    GridManager.instance.ClearNode(subNode);
                }
                GridManager.instance.ClearNode(node);
            }
            tacticalNodes.Clear();

            foreach (Node node in normalizedTacticalNodes)
            {
                foreach (Node subNode in normalizedTacticalNodes)
                {
                    GridManager.instance.ClearNode(subNode);
                }
                GridManager.instance.ClearNode(node);
            }
            normalizedTacticalNodes.Clear();
        }
        void ClearInvalidNodes()
        {
            foreach (Node node in InvalidNodes)
            {
                foreach (Node subNode in node.subNodes)
                {
                    GridManager.instance.ClearNode(subNode);
                }
                GridManager.instance.ClearNode(node);
            }
            InvalidNodes.Clear();
        }

        void DisableDragNDropComponent()
        {
            foreach (UnitController unit in FriendlyUnits)
            {
                DragNDropUnit draggable = unit.GetComponentInChildren<DragNDropUnit>();
                if (draggable)
                    draggable.enabled = false;
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

                    Node targetNode = GridManager.instance.GetNode(hit.point - offSet, currentUnit.GridIndex);

                    if (interactionHook != null)
                    {
                        if (isTargetPointBlank)
                            isTargetPointBlank = false;

                        if (!unitIsMoving)
                        {
                            if (targetNode != null)
                            {
                                if (FlowmapPathfinderMaster.instance.IsTargetNodeNeighbour(currentUnit.CurrentNode, targetNode))
                                {
                                    FlowmapPathfinderMaster.instance.ClearPathData();
                                    isTargetPointBlank = true;
                                }
                            }
                        }
                        //Will need to add check for distance if unit doesn't have full field range
                        if (interactionHook.interactionContainer != null && currentUnit.AttackType.Equals(Enums.UnitAttackType.RANGED))
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
                            }
                        }

                        else if ((interactionHook.interactionContainer != null && FlowmapPathfinderMaster.instance.previousPath.Count != 0) || isTargetPointBlank)
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
                        if (targetNode != null)
                        {
                            FlowmapPathfinderMaster.instance.UnwalkableNodes.Add(targetNode);
                        }
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
                if (ih != null && ih.gameObject.layer != currentUnit.gameObject.layer)
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
            if (EnemyUnitManager.instance.AiInteracting)
                EnemyUnitManager.instance.AiInteracting = false;

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
            FlowmapPathfinderMaster.instance.ClearUnwalkableNodes();

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
            if (unitController.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
            {
                EnemyUnitManager.instance.UpdateAiControllers(unitController);
            }
            unitsQueue.Remove(unitController);
            UiManager.instance.UpdateUiOnUnitDeath(unitController);
        }
    }
}