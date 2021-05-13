using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

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

        int currentDecisionStrength;

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
            if (AIsTargets[unitController] != null)
            {
                UnitController newTarget = ScanForTargetUnit(unitController);
                if (newTarget != null)
                {
                    AIsTargets[unitController] = newTarget;
                }
            }

            else if (AIsTargets[unitController] == null)
                AIsTargets[unitController] = SelectInitialTarget(unitController);

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

        UnitController SelectInitialTarget(UnitController unitController)
        {
            if (availableTargetUnits.Count <= 0)
                return null;

            UnitController retVal = null;

            float minDistance = float.MaxValue;

            foreach (UnitController target in availableTargetUnits)
            {
                float distance = Vector3.Distance(unitController.CurrentNode.worldPosition, target.CurrentNode.worldPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    retVal = target;
                }
            }

            currentDecisionStrength = (int)TargetPriority.INITITAL;
            return retVal;
        }

        void GetTargetNode(UnitController unitController)
        {
            List<Node> reachableNodes = FlowmapPathfinderMaster.instance.reachableNodes;

            float minDistance = float.MaxValue;

            for (int i = 0; i < reachableNodes.Count; i++)
            {
                float distance = Vector3.Distance(reachableNodes[i].position, AIsTargets[unitController].CurrentNode.position);
                if (IsTargetNodeNeighbour(reachableNodes[i], AIsTargets[unitController].CurrentNode))
                {
                    isInteractionInitialized = true;
                }
                if (distance < minDistance && reachableNodes[i].IsWalkable())
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

        UnitController ScanForTargetUnit(UnitController unitController)
        {
            List<UnitController> nearbyUnits = new List<UnitController>();

            Collider[] colliders = Physics.OverlapSphere(unitController.transform.position, 125);
            foreach (Collider c in colliders)
            {
                UnitController unit = c.transform.GetComponentInParent<UnitController>();

                if (unit != null && availableTargetUnits.Contains(unit))
                {
                    nearbyUnits.Add(unit);
                }
            }

            if (nearbyUnits.Count > 0)
                return RetargetOrRefuse(unitController, nearbyUnits);

            return null;
        }

        UnitController RetargetOrRefuse(UnitController unitController, List<UnitController> nearbyUnits)
        {
            Dictionary<UnitController, int> localTargetsScores = new Dictionary<UnitController, int>();
            UnitController retVal = null;

            int decisionPoints = 0;
            int targetThreshold = 5;

            foreach (UnitController unit in nearbyUnits)
            {
                decisionPoints += AddToDecision(new StatComparatorHolder(AIsTargets[unitController].HitPoints, unit.HitPoints), FloatExtension.Less, (int)ScoreValue.STANDARD);
                decisionPoints += AddToDecision(new StatComparatorHolder(AIsTargets[unitController].Defense, unit.Defense), FloatExtension.Less, (int)ScoreValue.STANDARD);

                decisionPoints += AddToDecision(new StatComparatorHolder(unit.HitPoints, unitController.Damage), FloatExtension.LessOrEqual, (int)ScoreValue.HIGH);

                StatComparatorHolder[] statHolders = new StatComparatorHolder[] {
                        new StatComparatorHolder(unit.HitPoints, unitController.Damage),
                        new StatComparatorHolder(unit.Damage, targetThreshold) };

                Func<float, float, bool>[] comparisons = new Func<float, float, bool>[] { FloatExtension.LessOrEqual, FloatExtension.More };

                decisionPoints += AddToDecision(statHolders, comparisons, (int)ScoreValue.TOP);
                decisionPoints += AddToDecision(unit, (int)ScoreValue.TOP);

                localTargetsScores.Add(unit, decisionPoints);

                decisionPoints = 0;
            }

            foreach (UnitController unit in localTargetsScores.Keys)
            {
                if (localTargetsScores[unit] < (int)TargetPriority.INITITAL)
                {
                    continue;
                }
                if (localTargetsScores[unit] >= (int)TargetPriority.TOP)
                {
                    currentDecisionStrength = (int)TargetPriority.TOP;
                    return unit;
                }
                if (localTargetsScores[unit] >= (int)TargetPriority.FAVOURABLE)
                {
                    currentDecisionStrength = (int)TargetPriority.FAVOURABLE;
                    retVal = unit;
                }
                if (localTargetsScores[unit] >= (int)TargetPriority.NEW)
                {
                    if (currentDecisionStrength < localTargetsScores[unit])
                    {
                        currentDecisionStrength = (int)TargetPriority.NEW;
                        retVal = unit;
                    }
                }
            }

            return retVal;
        }

        int AddToDecision(StatComparatorHolder statHolder, Func<float, float, bool> comparisonOperator, int value)
        {
            if (comparisonOperator.Invoke(statHolder.First, statHolder.Second))
            {
                return value;
            }

            return 0;
        }
        int AddToDecision(StatComparatorHolder[] statHolders, Func<float, float, bool>[] comparisons, int value)
        {
            int index = 0;
            foreach (StatComparatorHolder statHolder in statHolders)
            {
                if (!comparisons[index].Invoke(statHolder.First, statHolder.Second))
                {
                    return 0;
                }
                index++;
            }
            return value;
        }

        int AddToDecision(UnitController target, int value)
        {
            List<Node> reachableNodes = FlowmapPathfinderMaster.instance.reachableNodes;

            for (int i = 0; i < reachableNodes.Count; i++)
            {
                if (IsTargetNodeNeighbour(reachableNodes[i], target.CurrentNode))
                {
                    return value;
                }
            }
            return 0;
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