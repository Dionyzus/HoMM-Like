using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HOMM_BM
{
    public class MiniMax
    {
        List<UnitController> unitsQueue = new List<UnitController>();
        int depthSearch;
        UnitMove bestUnitMove;

        public MiniMax(List<UnitController> unitsQueue, int depth)
        {
            this.unitsQueue = unitsQueue;
            depthSearch = depth;
        }

        public UnitMove StartMiniMax()
        {
            List<SimpleUnit> simpleUnitsQueue = new List<SimpleUnit>();

            foreach (UnitController unit in unitsQueue)
            {
                UnitStatsReference unitStats = new UnitStatsReference(unit.HitPoints, unit.Damage, unit.Attack, unit.Defense, unit.Initiative, unit.StepsCount);

                SimpleUnit simpleUnit = new SimpleUnit(unitStats, unit.CurrentNode, unit.gameObject.layer, unit.GetInstanceID());

                simpleUnitsQueue.Add(simpleUnit);
            }
            GridState gridState = new GridState(simpleUnitsQueue, SimulationManager.instance.MoveCount, SimulationManager.instance.MoveCount);
            PerformMiniMax(gridState, depthSearch, -1000000, 1000000);

            return bestUnitMove;
        }

        int PerformMiniMax(GridState gridState, int depth, int alpha, int beta)
        {
            if (depth == 0 || gridState.IsGameOver())
            {
                return gridState.Evaluate();
            }

            if (gridState.CurrentSimple.Layer == GridManager.ENEMY_UNITS_LAYER)
            {
                List<UnitMove> legalMoves = gridState.GetLegalMoves();
                gridState.EvaluateMoves(legalMoves);

                legalMoves = legalMoves.OrderByDescending(move => move.MoveEvaluation).ToList();

                for (int i = 0, len = legalMoves.Count; i < len; ++i)
                {
                    GridState newGridState = new GridState(gridState);
                    newGridState.MoveUnit(legalMoves[i]);

                    int evaluation = PerformMiniMax(newGridState, depth - 1, alpha, beta);

                    if (evaluation > alpha)
                    {
                        alpha = evaluation;
                        if (depth == depthSearch)
                            bestUnitMove = legalMoves[i];
                    }
                    if (beta <= alpha)
                        break;
                }
                return alpha;
            }
            else
            {
                List<UnitMove> legalMoves = gridState.GetLegalMoves();
                gridState.EvaluateMoves(legalMoves);

                legalMoves = legalMoves.OrderByDescending(move => move.MoveEvaluation).ToList();

                for (int i = 0, len = legalMoves.Count; i < len; ++i)
                {
                    GridState newGridState = new GridState(gridState);
                    newGridState.MoveUnit(legalMoves[i]);

                    int evaluation = PerformMiniMax(newGridState, depth - 1, alpha, beta);

                    if (evaluation < beta)
                    {
                        beta = evaluation;
                    }
                    if (beta <= alpha)
                        break;
                }
                return beta;
            }
        }
    }
}