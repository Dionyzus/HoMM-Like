using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HOMM_BM
{
    public class WorldMiniMax
    {
        List<HeroController> heroesQueue = new List<HeroController>();
        int depthSearch;
        HeroMove bestHeroMove;

        public WorldMiniMax(List<HeroController> heroesQueue, int depth)
        {
            this.heroesQueue = heroesQueue;
            depthSearch = depth;
        }

        public HeroMove StartMiniMax()
        {
            List<SimulationHero> simulationHeroesQueue = new List<SimulationHero>();

            foreach (HeroController hero in heroesQueue)
            {
                UnitSide heroSide = 
                    hero.gameObject.layer == GridManager.ENEMY_UNITS_LAYER ? UnitSide.MAX_UNIT : UnitSide.MIN_UNIT;

                SimulationHero simulationHero = new SimulationHero(hero.CurrentNode, heroSide, hero.GetInstanceID(), hero.StepsCount);
                simulationHeroesQueue.Add(simulationHero);
            }

            WorldGridState gridState = new WorldGridState(simulationHeroesQueue, WorldSimulationManager.instance.MoveCount, WorldSimulationManager.instance.MoveCount);
            PerformMiniMax(gridState, depthSearch, -1000000, 1000000);

            return bestHeroMove;
        }

        int PerformMiniMax(WorldGridState gridState, int depth, int alpha, int beta)
        {
            if (depth == 0 || gridState.IsGameOver())
            {
                return gridState.Evaluate();
            }

            if (gridState.CurrentHero.HeroSide == UnitSide.MAX_UNIT)
            {
                List<HeroMove> legalMoves = gridState.GetLegalMoves();
                gridState.EvaluateMoves(legalMoves);

                legalMoves = legalMoves.OrderByDescending(move => move.MoveEvaluation).ToList();

                for (int i = 0, len = legalMoves.Count; i < len; ++i)
                {
                    WorldGridState newGridState = new WorldGridState(gridState);
                    newGridState.MoveHero(legalMoves[i]);

                    int evaluation = PerformMiniMax(newGridState, depth - 1, alpha, beta);

                    if (evaluation > alpha)
                    {
                        alpha = evaluation;
                        if (depth == depthSearch)
                            bestHeroMove = legalMoves[i];
                    }
                    if (beta <= alpha)
                        break;
                }
                return alpha;
            }
            else
            {
                List<HeroMove> legalMoves = gridState.GetLegalMoves();
                gridState.EvaluateMoves(legalMoves);

                legalMoves = legalMoves.OrderByDescending(move => move.MoveEvaluation).ToList();

                for (int i = 0, len = legalMoves.Count; i < len; ++i)
                {
                    WorldGridState newGridState = new WorldGridState(gridState);
                    newGridState.MoveHero(legalMoves[i]);

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