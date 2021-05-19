﻿using System.Collections.Generic;
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
                SimpleUnit newSimpleUnit = new SimpleUnit(unit.UnitStats, unit.CurrentNode, unit.Layer, unit.UnitId);

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
                if (unit.Layer == currentSimple.Layer)
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

                        newAttackMove.TargetAvailableFromNode = new SimpleUnit(target.UnitStats, target.CurrentNode, target.Layer, target.UnitId);

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

            //if (availableTargets.Count > 0)
            //    EvaluateAvailableTargets(availableTargets);

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

        Dictionary<SimpleUnit, int> EvaluateAvailableTargets(List<SimpleUnit> availableTargets)
        {
            Dictionary<SimpleUnit, int> targetsPriorityScores = new Dictionary<SimpleUnit, int>();

            int priorityScore = 0;

            SimpleUnit currentTarget = availableTargets.ElementAt(0);
            targetsPriorityScores.Add(currentTarget, (int)TargetPriority.INITITAL);

            foreach (SimpleUnit unit in availableTargets)
            {
                if (unit.UnitId == currentTarget.UnitId)
                    continue;

                priorityScore += AddToDecision(new StatComparatorHolder(currentTarget.HitPoints, unit.HitPoints), FloatExtension.Less, (int)ScoreValue.STANDARD);
                priorityScore += AddToDecision(new StatComparatorHolder(currentTarget.Defense, unit.Defense), FloatExtension.Less, (int)ScoreValue.STANDARD);
                priorityScore += AddToDecision(new StatComparatorHolder(unit.HitPoints, currentSimple.Damage), FloatExtension.LessOrEqual, (int)ScoreValue.HIGH);

                StatComparatorHolder[] statHolders = new StatComparatorHolder[] {
                        new StatComparatorHolder(unit.HitPoints, currentSimple.Damage),
                        new StatComparatorHolder(unit.Damage, currentSimple.HitPoints) };

                Func<float, float, bool>[] comparisons = new Func<float, float, bool>[] { FloatExtension.LessOrEqual, FloatExtension.More };

                priorityScore += AddToDecision(statHolders, comparisons, (int)ScoreValue.TOP);
                priorityScore += AddToDecision(unit, (int)ScoreValue.TOP);

                targetsPriorityScores.Add(unit, priorityScore);

                if (priorityScore > targetsPriorityScores[currentTarget])
                {
                    currentTarget = unit;
                }
            }

            foreach (SimpleUnit unit in targetsPriorityScores.Keys)
            {
                if (targetsPriorityScores[unit] >= (int)TargetPriority.TOP)
                {
                    targetsPriorityScores[unit] = (int)TargetPriority.TOP;
                }
                if (targetsPriorityScores[unit] >= (int)TargetPriority.FAVOURABLE)
                {
                    targetsPriorityScores[unit] = (int)TargetPriority.FAVOURABLE;
                }
                if (targetsPriorityScores[unit] >= (int)TargetPriority.NEW)
                {
                    targetsPriorityScores[unit] = (int)TargetPriority.NEW;
                }
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
            int maxTotalHitPoints = CalculateHitPoints(GridManager.ENEMY_UNITS_LAYER);
            int minTotalHitPoints = CalculateHitPoints(GridManager.FRIENDLY_UNITS_LAYER);
            int unitsTotalHitPointsDifference = maxTotalHitPoints - minTotalHitPoints;

            if (maxTotalHitPoints == 0)
            {
                return -10000 + (moveCount - initialMoveCount);
            }
            else if (minTotalHitPoints == 0)
            {
                return 10000 - (moveCount - initialMoveCount);
            }

            int maxUnitsCount = GetUnitsOfLayer(GridManager.ENEMY_UNITS_LAYER).Count;
            int minUnitsCount = GetUnitsOfLayer(GridManager.FRIENDLY_UNITS_LAYER).Count;
            int unitsCountDifference = maxUnitsCount - minUnitsCount;

            int maxUnitsScore = CalculateScore(GridManager.ENEMY_UNITS_LAYER);
            int minUnitsScore = CalculateScore(GridManager.FRIENDLY_UNITS_LAYER);
            int unitsScoresDifference = maxUnitsScore - minUnitsScore;

            int totalEvaluation = unitsTotalHitPointsDifference;
            totalEvaluation += unitsScoresDifference * 5;
            totalEvaluation += unitsCountDifference * 10;

            int maxTotalDistance = CalculateDistancesToTargets(GridManager.ENEMY_UNITS_LAYER, GridManager.FRIENDLY_UNITS_LAYER);

            return totalEvaluation - maxTotalDistance;
        }

        private int CalculateScore(int layer)
        {
            int retVal = 0;
            foreach (SimpleUnit unit in unitsQueue)
            {
                if (unit.Layer == layer)
                {
                    retVal += unit.Defense;
                    retVal += unit.Attack;
                    retVal += unit.Initiative * 2;
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
                if (unit.Layer == 10)
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

        int CalculateHitPoints(int layer)
        {
            int retVal = 0;

            foreach (SimpleUnit unit in unitsQueue)
            {
                if (unit.Layer == layer)
                    retVal += unit.HitPoints;
            }

            return retVal;
        }

        int CalculateDistancesToTargets(int alliesLayer, int axisLayer)
        {
            List<SimpleUnit> allyUnits = GetUnitsOfLayer(alliesLayer);
            List<SimpleUnit> axisUnits = GetUnitsOfLayer(axisLayer);

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

        List<SimpleUnit> GetUnitsOfLayer(int layer)
        {
            List<SimpleUnit> units = new List<SimpleUnit>();

            foreach (SimpleUnit unit in unitsQueue)
            {
                if(unit.Layer == layer)
                {
                    units.Add(unit);
                }
            }

            return units;
        }
    }
}