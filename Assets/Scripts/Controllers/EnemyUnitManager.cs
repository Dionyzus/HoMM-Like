using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HOMM_BM
{
    public class EnemyUnitManager : MonoBehaviour
    {
        public static EnemyUnitManager instance;
        static System.Random rnd;

        bool isInitialized;

        Dictionary<UnitController, UnitController> AIsTargets = new Dictionary<UnitController, UnitController>();

        List<UnitController> availableAiControllers = new List<UnitController>();
        List<UnitController> availableTargetUnits = new List<UnitController>();

        Node targetNode;
        bool isInteractionInitialized;
        bool isTargetPointBlank;

        bool aiInteracting;
        public bool AiInteracting { get => aiInteracting; set => aiInteracting = value; }
        bool isTargetUnitDead;
        public bool IsTargetUnitDead { get => isTargetUnitDead; set => isTargetUnitDead = value; }

        void Awake()
        {
            rnd = new System.Random();
            instance = this;
        }

        public void Initialize()
        {
            if (!isInitialized)
            {
                availableTargetUnits = BattleManager.instance.FriendlyUnits;
                availableAiControllers = BattleManager.instance.EnemyUnits;

                InitializeAiControllers();

                isInitialized = true;
            }
        }
        public void InitializeAiControllers()
        {
            foreach (UnitController ai in availableAiControllers)
            {
                AIsTargets.Add(ai, null);
            }
        }
        public void UpdateAiControllers(UnitController target)
        {
            foreach (UnitController ai in AIsTargets.Keys.ToList())
            {
                if (AIsTargets[ai] != null)
                {
                    if (AIsTargets[ai].Equals(target))
                    {
                        AIsTargets[ai] = null;
                    }
                }
            }
            availableTargetUnits.Remove(target);
        }
        public void HandleAiTurn(UnitController unitController)
        {
            if (AIsTargets[unitController] == null)
                AIsTargets[unitController] = SelectTarget();

            if (AIsTargets[unitController] != null)
            {
                GetTargetNode(unitController);

                if (isInteractionInitialized)
                {
                    isInteractionInitialized = false;
                    HandeAiAction(unitController);
                    return;
                }
                if (targetNode != null)
                {
                    FlowmapPathfinderMaster.instance.GetPathFromMap(targetNode, unitController);
                    FlowmapPathfinderMaster.instance.InitiateMovingOnPath(unitController);
                }
                else
                {
                    Debug.Log("Couldn't find any valid path!");
                }
            }

            if (availableTargetUnits.Count <= 0)
            {
                Debug.Log("Ai won!");
            }
        }

        UnitController SelectTarget()
        {
            if (availableTargetUnits.Count <= 0)
                return null;

            int randomUnitIndex = rnd.Next(0, availableTargetUnits.Count - 1);

            return availableTargetUnits[randomUnitIndex];
        }

        void GetTargetNode(UnitController unitController)
        {
            List<Node> reachableNodes = FlowmapPathfinderMaster.instance.reachableNodes;

            float minDistance = float.MaxValue;

            for (int i = 0; i < reachableNodes.Count; i++)
            {
                float distance = GetDistance(reachableNodes[i].position, AIsTargets[unitController].CurrentNode.position);

                if (IsTargetNodeNeighbour(reachableNodes[i], AIsTargets[unitController].CurrentNode))
                {
                    isInteractionInitialized = true;
                }
                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetNode = reachableNodes[i];
                }
            }

            if (isInteractionInitialized)
            {
                FlowmapPathfinderMaster.instance.GetPathFromMap(targetNode, unitController);
                if (FlowmapPathfinderMaster.instance.previousPath.Count > 0)
                {
                    FlowmapPathfinderMaster.instance.InitiateMovingOnPath(unitController);
                }
                else if (IsTargetNodeNeighbour(targetNode, AIsTargets[unitController].CurrentNode))
                {
                    isTargetPointBlank = true;
                }
            }
        }

        public void HandeAiAction(UnitController unitController)
        {
            InteractionHook ih = AIsTargets[unitController].GetComponentInChildren<InteractionHook>();

            if (ih == null || !ih.enabled)
            {
                AIsTargets[unitController] = null;
                return;
            }

            unitController.currentInteractionHook = ih;
            unitController.IsInteractionInitialized = true;
            unitController.CreateInteractionContainer(unitController.currentInteractionHook.interactionContainer);

            BattleManager.instance.CurrentCombatEvent = new CombatEvent(unitController, AIsTargets[unitController]);

            if (BattleManager.instance.unitIsMoving)
            {
                aiInteracting = true;
                return;
            }
            else if (isTargetPointBlank)
            {
                aiInteracting = true;
                unitController.IsTargetPointBlank = true;

                isTargetPointBlank = false;
                FlowmapPathfinderMaster.instance.ClearFlowmapData();
            }
        }
        float GetDistance(Vector3 origin, Vector3 target)
        {
            return Vector3.Distance(origin, target);
        }
        public bool IsTargetNodeNeighbour(Node currentNode, Node targetNode)
        {
            float xDistance = Mathf.Abs(currentNode.position.x - targetNode.position.x);
            float zDistance = Mathf.Abs(currentNode.position.z - targetNode.position.z);

            if (xDistance <= 1 && zDistance <= 1)
            {
                return true;
            }
            return false;
        }
    }
}