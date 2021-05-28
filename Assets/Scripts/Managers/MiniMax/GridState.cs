using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace HOMM_BM
{
    public class GridState
    {
        SimpleUnit currentSimple;
        List<SimpleUnit> unitsQueue = new List<SimpleUnit>();

        public SimpleUnit CurrentSimple { get => currentSimple; set => currentSimple = value; }
        public List<SimpleUnit> UnitsQueue { get => unitsQueue; set => unitsQueue = value; }

        public int initialMoveCount;
        public int moveCount;

        public GridState(List<SimpleUnit> unitsQueue, int moveCount, int initialMoveCount)
        {
            this.moveCount = moveCount;
            this.initialMoveCount = initialMoveCount;

            this.unitsQueue = unitsQueue;
            currentSimple = this.unitsQueue.First();
        }

        public GridState(GridState gridState)
        {
            moveCount = gridState.moveCount;
            initialMoveCount = gridState.initialMoveCount;

            unitsQueue = new List<SimpleUnit>();

            foreach (SimpleUnit unit in gridState.unitsQueue)
            {
                SimpleUnit newSimpleUnit = new SimpleUnit(unit.UnitStats, unit.CurrentNode, unit.UnitSide, unit.UnitId);

                unitsQueue.Add(newSimpleUnit);
            }

            currentSimple = unitsQueue.First();
        }

        public void MoveUnit(UnitMove unitMove)
        {
            if (unitMove.TargetNode.IsWalkable())
            {
                if (unitMove.IsAttackMove)
                {
                    currentSimple.CurrentNode = unitMove.TargetNode;

                    foreach (SimpleUnit unit in unitsQueue)
                    {
                        if (unit.UnitId == unitMove.TargetAvailableFromNode.UnitId)
                        {
                            unit.HitPoints -= currentSimple.Damage;

                            if (unit.HitPoints <= 0)
                            {
                                unitsQueue.Remove(unit);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    currentSimple.CurrentNode = unitMove.TargetNode;
                }
            }

            unitsQueue.Remove(currentSimple);
            unitsQueue.Add(currentSimple);

            currentSimple = unitsQueue.First();

            moveCount++;
        }

        private List<SimpleUnit> FindAvailableTargets()
        {
            List<SimpleUnit> availableTargets = new List<SimpleUnit>();

            foreach (SimpleUnit unit in unitsQueue)
            {
                if (unit.UnitSide == currentSimple.UnitSide)
                    continue;
                else
                {
                    availableTargets.Add(unit);
                }
            }

            return availableTargets;
        }

        List<Node> CalculateWalkablePositions()
        {
            if (currentSimple == null)
            {
                Debug.Log("Current unit is null!");
                return new List<Node>();
            }

            return CreateFlowmapForNode();
        }

        List<UnitMove> ConvertNodesToUnitMoves(List<Node> reachableNodes)
        {
            List<SimpleUnit> availableTargets = FindAvailableTargets();
            List<UnitMove> legalMoves = new List<UnitMove>();

            for (int i = 0, len = reachableNodes.Count; i < len; ++i)
            {
                foreach (SimpleUnit target in availableTargets)
                {
                    if (IsTargetNodeNeighbour(target.CurrentNode, reachableNodes[i]))
                    {
                        UnitMove newAttackMove = new UnitMove();

                        newAttackMove.IsAttackMove = true;
                        newAttackMove.TargetNode = reachableNodes[i];

                        newAttackMove.TargetAvailableFromNode = new SimpleUnit(target.UnitStats, target.CurrentNode, target.UnitSide, target.UnitId);

                        legalMoves.Add(newAttackMove);
                    }
                }

                UnitMove newLegalMove = new UnitMove();
                newLegalMove.TargetNode = reachableNodes[i];

                legalMoves.Add(newLegalMove);
            }

            return legalMoves;
        }

        public void EvaluateMoves(List<UnitMove> unitMoves)
        {
            List<SimpleUnit> availableTargets = FindAvailableTargets();

            Dictionary<SimpleUnit, TargetPriority> evaluatedTargets = new Dictionary<SimpleUnit, TargetPriority>();

            if (availableTargets.Count > 0)
                evaluatedTargets = EvaluateAvailableTargets(availableTargets);

            List<UnitMoveInfo> unitMovesInfo = new List<UnitMoveInfo>();

            foreach (SimpleUnit target in availableTargets)
            {
                UnitMoveInfo moveInfo = new UnitMoveInfo(target, unitMoves[0], float.MaxValue);
                unitMovesInfo.Add(moveInfo);
            }

            for (int i = 0, len = unitMoves.Count; i < len; ++i)
            {
                if (unitMoves[i].IsAttackMove)
                {
                    unitMoves[i].MoveEvaluation += (int)EvaluationScore.ATTACK_MOVE;

                    foreach (SimpleUnit target in evaluatedTargets.Keys)
                    {
                        if (target.UnitId == unitMoves[i].TargetAvailableFromNode.UnitId)
                        {
                            unitMoves[i].MoveEvaluation += (int)evaluatedTargets[target];
                            break;
                        }
                    }
                }
                else
                {
                    foreach (UnitMoveInfo unitMoveInfo in unitMovesInfo)
                    {
                        float newDistance = Vector3.Distance(unitMoves[i].TargetNode.worldPosition, unitMoveInfo.Unit.CurrentNode.worldPosition);

                        if (newDistance < unitMoveInfo.Distance)
                        {
                            unitMoveInfo.Distance = newDistance;
                            unitMoveInfo.UnitMove = unitMoves[i];
                        }
                    }
                }
            }

            foreach (UnitMoveInfo unitMoveInfo in unitMovesInfo)
            {
                for (int i = 0, len = unitMoves.Count; i < len; ++i)
                {
                    if (unitMoves[i] == unitMoveInfo.UnitMove)
                    {
                        if (unitMoves[i].MoveEvaluation == 1)
                            unitMoves[i].MoveEvaluation += (int)EvaluationScore.ROAMING_MOVE;
                        break;
                    }
                }
            }
        }

        Dictionary<SimpleUnit, TargetPriority> EvaluateAvailableTargets(List<SimpleUnit> availableTargets)
        {
            Dictionary<SimpleUnit, TargetPriority> targetsPriorityScores = new Dictionary<SimpleUnit, TargetPriority>();

            foreach (SimpleUnit target in availableTargets)
            {
                targetsPriorityScores.Add(target, TargetPriority.INITITAL);
            }

            SimpleUnit currentTarget = availableTargets[0];
            int priorityScore = (int)TargetPriority.INITITAL;

            foreach (SimpleUnit target in availableTargets)
            {
                priorityScore += AddToDecision(new StatComparatorHolder(target.HitPoints, currentTarget.HitPoints), FloatExtension.Less, (int)ScoreValue.STANDARD);
                priorityScore += AddToDecision(new StatComparatorHolder(target.Defense, currentTarget.Defense), FloatExtension.Less, (int)ScoreValue.STANDARD);
                priorityScore += AddToDecision(new StatComparatorHolder(target.HitPoints, currentSimple.Damage), FloatExtension.LessOrEqual, (int)ScoreValue.HIGH);

                StatComparatorHolder[] statHolders = new StatComparatorHolder[] {
                        new StatComparatorHolder(target.HitPoints, currentSimple.Damage),
                        new StatComparatorHolder(target.Damage, currentSimple.HitPoints) };

                Func<float, float, bool>[] comparisons = new Func<float, float, bool>[] { FloatExtension.LessOrEqual, FloatExtension.More };

                priorityScore += AddToDecision(statHolders, comparisons, (int)ScoreValue.TOP);
                priorityScore += AddToDecision(target, (int)ScoreValue.TOP);

                targetsPriorityScores[target] = priorityScore.ConvertToTargetPriority();

                if (currentTarget != target && priorityScore > (int)targetsPriorityScores[currentTarget])
                {
                    currentTarget = target;
                }

                priorityScore = (int)TargetPriority.INITITAL;
            }

            return targetsPriorityScores;
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

        int AddToDecision(SimpleUnit target, int value)
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

        List<Node> CreateFlowmapForNode()
        {
            List<Node> reachableNodes = new List<Node>();
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            currentSimple.CurrentNode.steps = 0;

            reachableNodes.Add(currentSimple.CurrentNode);
            openSet.Add(currentSimple.CurrentNode);
            
            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                int steps = currentNode.steps;
                steps++;

                if (steps <= currentSimple.StepsCount)
                {
                    foreach (Node node in GetNeighbours(currentNode, steps))
                    {
                        if (!closedSet.Contains(node))
                        {
                            if (!openSet.Contains(node))
                            {
                                openSet.Add(node);
                                node.steps = steps;

                                if (node.steps <= currentSimple.StepsCount)
                                    reachableNodes.Add(node);
                            }
                        }
                    }
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
            }
            return reachableNodes;
        }

        List<Node> GetNeighbours(Node currentNode, int steps)
        {
            List<Node> retVal = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && z == 0)
                        continue;

                    if (x == z && steps == currentSimple.StepsCount - 1)
                    {
                        continue;
                    }
                    if (x == -1 && z == 1 && steps == currentSimple.StepsCount - 1)
                    {
                        continue;
                    }
                    if (x == 1 && z == -1 && steps == currentSimple.StepsCount - 1)
                    {
                        continue;
                    }

                    int _x = x + currentNode.position.x;
                    int _y = currentNode.position.y;
                    int _z = z + currentNode.position.z;

                    if (_x == currentNode.position.x && _z == currentNode.position.z)
                        continue;

                    Node node = GridManager.instance.GetNode(_x, _y, _z, currentSimple.CurrentNode.gridIndex, true);

                    if (_x == currentSimple.CurrentNode.position.x &&
                                _z == currentSimple.CurrentNode.position.z)
                    {
                        retVal.Add(currentSimple.CurrentNode);
                    }
                    else if (node != null && node.IsWalkable())
                    {
                        retVal.Add(node);
                    }
                }
            }
            return retVal;
        }
        public List<UnitMove> GetLegalMoves()
        {
            return ConvertNodesToUnitMoves(CalculateWalkablePositions());
        }

        bool IsTargetNodeNeighbour(Node currentNode, Node targetNode)
        {
            float xDistance = Mathf.Abs(currentNode.worldPosition.x - targetNode.worldPosition.x);
            float zDistance = Mathf.Abs(currentNode.worldPosition.z - targetNode.worldPosition.z);

            if (xDistance <= 1 && zDistance <= 1)
            {
                return true;
            }
            return false;
        }

        public int Evaluate()
        {
            int maxTotalHitPoints = CalculateHitPoints(UnitSide.MAX_UNIT);
            int minTotalHitPoints = CalculateHitPoints(UnitSide.MIN_UNIT);
            int unitsTotalHitPointsDifference = maxTotalHitPoints - minTotalHitPoints;

            if (maxTotalHitPoints == 0)
            {
                return -10000 + (moveCount - initialMoveCount);
            }
            else if (minTotalHitPoints == 0)
            {
                return 10000 - (moveCount - initialMoveCount);
            }

            int maxUnitsCount = GetUnitsOfLayer(UnitSide.MAX_UNIT).Count;
            int minUnitsCount = GetUnitsOfLayer(UnitSide.MIN_UNIT).Count;
            int unitsCountDifference = maxUnitsCount - minUnitsCount;

            int maxStatsScore = CalculateStatsScore(UnitSide.MAX_UNIT);
            int minStatsScore = CalculateStatsScore(UnitSide.MIN_UNIT);
            int statsScoreDifference = maxStatsScore - minStatsScore;

            int totalEvaluation = unitsTotalHitPointsDifference;
            totalEvaluation += statsScoreDifference * (int)EvaluationBoost.STATS_SCORE;
            totalEvaluation += unitsCountDifference * (int)EvaluationBoost.UNITS_COUNT;

            int maxTotalDistance = CalculateDistancesToTargets(UnitSide.MIN_UNIT, UnitSide.MAX_UNIT);

            return totalEvaluation - maxTotalDistance;
        }

        private int CalculateStatsScore(UnitSide unitSide)
        {
            int retVal = 0;
            foreach (SimpleUnit unit in unitsQueue)
            {
                if (unit.UnitSide == unitSide)
                {
                    retVal += unit.Defense;
                    retVal += unit.Attack;
                    retVal += unit.Initiative * (int)EvaluationBoost.INITIATIVE;
                }
            }

            return retVal;
        }

        public bool IsGameOver()
        {
            int aiSimplesCount = 0;
            int humanSimplesCount = 0;

            foreach (SimpleUnit unit in unitsQueue)
            {
                if (unit.UnitSide == UnitSide.MAX_UNIT)
                {
                    aiSimplesCount += 1;
                }
                else
                {
                    humanSimplesCount += 1;
                }
            }

            if (aiSimplesCount == 0 || humanSimplesCount == 0)
            {
                return true;
            }
            return false;
        }

        int CalculateHitPoints(UnitSide unitSide)
        {
            int retVal = 0;

            foreach (SimpleUnit unit in unitsQueue)
            {
                if (unit.UnitSide == unitSide)
                    retVal += unit.HitPoints;
            }

            return retVal;
        }

        int CalculateDistancesToTargets(UnitSide minSide, UnitSide maxSide)
        {
            List<SimpleUnit> allyUnits = GetUnitsOfLayer(minSide);
            List<SimpleUnit> axisUnits = GetUnitsOfLayer(maxSide);

            int totalDistance = 0;

            foreach (SimpleUnit ally in allyUnits)
            {
                float distanceToClosestTarget = float.MaxValue;

                foreach (SimpleUnit axis in axisUnits)
                {
                    float distance = Mathf.Abs(Vector3.Distance(ally.CurrentNode.worldPosition, axis.CurrentNode.worldPosition));

                    if (distance < distanceToClosestTarget)
                    {
                        distanceToClosestTarget = distance;
                    }
                }

                totalDistance += (int)distanceToClosestTarget;
            }

            return totalDistance;
        }

        List<SimpleUnit> GetUnitsOfLayer(UnitSide unitSide)
        {
            List<SimpleUnit> units = new List<SimpleUnit>();

            foreach (SimpleUnit unit in unitsQueue)
            {
                if(unit.UnitSide == unitSide)
                {
                    units.Add(unit);
                }
            }

            return units;
        }
    }
}