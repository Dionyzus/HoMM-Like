using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HOMM_BM
{
    public class SimulationManager : MonoBehaviour
    {
        public static SimulationManager instance;

        UnitController currentUnit;
        List<UnitController> unitsQueue = new List<UnitController>();

        bool isInteractionInitialized;
        bool isTargetPointBlank;
        bool aiInteracting;
        public bool AiInteracting { get => aiInteracting; set => aiInteracting = value; }

        public void Awake()
        {
            instance = this;
        }

        public void Initialize()
        {
            unitsQueue = new List<UnitController>(BattleManager.instance.UnitsQueue);
            currentUnit = unitsQueue.First();

            MiniMax minimax = new MiniMax(unitsQueue, 1);
            UnitMove unitMove = minimax.StartMiniMax();

            if (unitMove == null)
            {
                Debug.Log("AI won!");
                aiInteracting = true;
                return;
            }

            if (unitMove.SimpleUnit == null)
            {
                Debug.Log("No unit for the move!");
            }

            if (unitMove.TargetSimple != null)
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
                Debug.Log("Unit: " + unitMove.SimpleUnit.Layer + " In want to walk: " + unitMove.TargetNode.worldPosition);
                FlowmapPathfinderMaster.instance.GetPathFromMap(unitMove.TargetNode, currentUnit);
                FlowmapPathfinderMaster.instance.InitiateMovingOnPath(currentUnit);
            }

            if (isInteractionInitialized)
            {
                isInteractionInitialized = false;
                HandeAiAction(unitMove);

                unitMove.TargetSimple = null;
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
        void HandeAiAction(UnitMove unitMove)
        {
            InteractionHook ih = unitMove.TargetUnit.GetComponentInChildren<InteractionHook>();

            if (ih == null || !ih.enabled)
            {
                unitMove.TargetUnit = null;
                return;
            }

            currentUnit.currentInteractionHook = ih;
            currentUnit.IsInteractionInitialized = true;
            currentUnit.CreateInteractionContainer(currentUnit.currentInteractionHook.interactionContainer);

            BattleManager.instance.CurrentCombatEvent = new CombatEvent(currentUnit, unitMove.TargetUnit);

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