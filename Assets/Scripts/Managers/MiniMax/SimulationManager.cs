using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HOMM_BM
{
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager instance;

        private int moveCount;

        UnitController currentUnit;
        UnitController targetUnit;

        List<UnitController> unitsQueue = new List<UnitController>();

        bool isInteractionInitialized;
        bool isTargetPointBlank;
        bool aiInteracting;
        public bool AiInteracting { get => aiInteracting; set => aiInteracting = value; }
        public int MoveCount { get => moveCount; set => moveCount = value; }

        public void Awake()
        {
            instance = this;
        }

        public void Initialize()
        {
            unitsQueue = new List<UnitController>(BattleManager.instance.UnitsQueue);
            currentUnit = unitsQueue.First();

            MiniMax minimax = new MiniMax(unitsQueue, 3);
            UnitMove unitMove = minimax.StartMiniMax();

            MoveCount++;

            if (unitMove == null)
            {
                Debug.Log("AI won!");
                aiInteracting = true;
                return;
            }

            if (unitMove.TargetAvailableFromNode != null)
            {
                foreach (UnitController unit in unitsQueue)
                {
                    if (unitMove.TargetAvailableFromNode.UnitId.Equals(unit.GetInstanceID()))
                    {
                        targetUnit = unit;
                    }
                }
            }

            if (targetUnit != null)
            {
                isInteractionInitialized = true;

                FlowmapPathfinderMaster.instance.GetPathFromMap(unitMove.TargetNode, currentUnit);
                if (FlowmapPathfinderMaster.instance.previousPath.Count > 0)
                {
                    FlowmapPathfinderMaster.instance.InitiateMovingOnPath(currentUnit);
                }
                else if (IsTargetNodeNeighbour(unitMove.TargetNode, currentUnit.CurrentNode))
                {
                    isTargetPointBlank = true;
                }
            }
            else
            {
                aiInteracting = true;
                if (currentUnit.CurrentNode == unitMove.TargetNode)
                {
                    BattleManager.instance.OnMoveFinished();
                }
                else
                {
                    FlowmapPathfinderMaster.instance.GetPathFromMap(unitMove.TargetNode, currentUnit);
                    FlowmapPathfinderMaster.instance.InitiateMovingOnPath(currentUnit);
                }
            }

            if (isInteractionInitialized)
            {
                isInteractionInitialized = false;
                HandeAiAction();

                if (targetUnit != null)
                    targetUnit = null;
            }
        }
        bool IsTargetNodeNeighbour(Node currentNode, Node targetNode)
        {
            float xDistance = Mathf.Abs(currentNode.position.x - targetNode.position.x);
            float zDistance = Mathf.Abs(currentNode.position.z - targetNode.position.z);

            if (xDistance <= 1 && zDistance <= 1)
            {
                return true;
            }
            return false;
        }
        void HandeAiAction()
        {
            InteractionHook ih = targetUnit.GetComponentInChildren<InteractionHook>();

            if (ih == null || !ih.enabled)
            {
                targetUnit = null;
                return;
            }

            currentUnit.currentInteractionHook = ih;
            currentUnit.IsInteractionInitialized = true;
            currentUnit.CreateInteractionContainer(currentUnit.currentInteractionHook.interactionContainer);

            BattleManager.instance.CurrentCombatEvent = new CombatEvent(currentUnit, targetUnit);

            if (BattleManager.instance.unitIsMoving)
            {
                aiInteracting = true;
                return;
            }
            else if (isTargetPointBlank)
            {
                aiInteracting = true;
                currentUnit.IsTargetPointBlank = true;

                isTargetPointBlank = false;
                FlowmapPathfinderMaster.instance.ClearFlowmapData();
            }
        }
    }
}