using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class WorldMiniMax
    {
        List<HeroController> heroesQueue = new List<HeroController>();
        int depthSearch;
        Node bestHeroMove;

        public WorldMiniMax(List<HeroController> heroesQueue, int depth)
        {
            this.heroesQueue = heroesQueue;
            depthSearch = depth;
        }

        public Node StartMiniMax()
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
                List<Node> legalMoves = gridState.GetLegalMoves();

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
                List<Node> legalMoves = gridState.GetLegalMoves();

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