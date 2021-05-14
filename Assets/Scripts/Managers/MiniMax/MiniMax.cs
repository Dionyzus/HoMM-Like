using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HOMM_BM
{
    public class MiniMax
    {
        List<UnitController> unitsQueue = new List<UnitController>();
        int depth;

        public MiniMax(List<UnitController> unitsQueue, int depth)
        {
            this.unitsQueue = unitsQueue;
            this.depth = depth;
        }

        public UnitMove StartMiniMax()
        {
            List<SimpleUnit> simpleUnitsQueue = new List<SimpleUnit>();

            foreach (UnitController unit in unitsQueue)
            {
                SimpleUnit simpleUnit = new SimpleUnit(unit.unitStats, unit.CurrentNode, unit.gameObject.layer);
                simpleUnitsQueue.Add(simpleUnit);
            }

            GridState gridState = new GridState(simpleUnitsQueue);
            (int evaluation, UnitMove unitMove) miniMaxResult = PerformMiniMax(gridState, depth, -1000, 1000);

            return miniMaxResult.unitMove;
        }

        (int, UnitMove) PerformMiniMax(GridState gridState, int depth, int alpha, int beta)
        {
            if (depth == 0 || gridState.IsGameOver())
            {
                return (gridState.Evaluate(), null);
            }

            List<Node> legalMoves = gridState.UpdateUnitMoves();

            if (gridState.CurrentSimple.Layer == GridManager.ENEMY_UNITS_LAYER)
            {
                UnitMove maximizerBestMove = null;

                foreach (Node node in legalMoves)
                {
                    GridState newGridState = new GridState(gridState.UnitsQueue);
                    UnitMove unitMove = new UnitMove(newGridState.CurrentSimple, newGridState.DebugSimple, node);

                    newGridState.MoveUnit(node);

                    (int evaluation, UnitMove unitMove) miniMaxResult = PerformMiniMax(newGridState, depth - 1, alpha, beta);

                    if (alpha < miniMaxResult.evaluation)
                    {
                        alpha = miniMaxResult.evaluation;
                        maximizerBestMove = unitMove;
                    }
                    if (beta <= alpha)
                        break;
                }
                return (alpha, maximizerBestMove);
            }
            else
            {
                UnitMove minimizerBestMove = null;

                foreach (Node node in legalMoves)
                {
                    GridState newGridState = new GridState(gridState.UnitsQueue);
                    UnitMove unitMove = new UnitMove(newGridState.CurrentSimple, newGridState.DebugSimple, node);

                    newGridState.MoveUnit(node);

                    (int evaluation, UnitMove unitMove) miniMaxResult = PerformMiniMax(newGridState, depth - 1, alpha, beta);

                    if (beta > miniMaxResult.evaluation)
                    {
                        beta = miniMaxResult.evaluation;
                        minimizerBestMove = unitMove;
                    }
                    if (beta <= alpha)
                        break;
                }
                return (beta, minimizerBestMove);
            }
        }
    }
}