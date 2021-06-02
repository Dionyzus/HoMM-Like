using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HOMM_BM
{
    public class WorldGridState
    {
        public int initialMoveCount;
        public int moveCount;

        SimulationHero currentHero;
        List<SimulationHero> heroesQueue = new List<SimulationHero>();

        public SimulationHero CurrentHero { get => currentHero; set => currentHero = value; }
        public List<SimulationHero> HeroesQueue { get => heroesQueue; set => heroesQueue = value; }

        public WorldGridState(List<SimulationHero> heroesQueue, int moveCount, int initialMoveCount)
        {
            this.moveCount = moveCount;
            this.initialMoveCount = initialMoveCount;

            this.heroesQueue = heroesQueue;
            currentHero = this.heroesQueue.First();
        }

        public WorldGridState(WorldGridState gridState)
        {
            moveCount = gridState.moveCount;
            initialMoveCount = gridState.initialMoveCount;

            heroesQueue = new List<SimulationHero>();

            foreach (SimulationHero hero in gridState.heroesQueue)
            {
                SimulationHero newSimulationHero = new SimulationHero(hero.CurrentNode, hero.HeroSide, hero.HeroId, hero.StepsCount);

                heroesQueue.Add(newSimulationHero);
            }

            currentHero = heroesQueue.First();
        }
        public List<Node> GetLegalMoves()
        {
            return CalculateWalkablePositions();
        }
        List<Node> CalculateWalkablePositions()
        {
            if (currentHero == null)
            {
                Debug.Log("Current hero is null!");
                return new List<Node>();
            }

            return CreateFlowmapForNode();
        }

        List<Node> CreateFlowmapForNode()
        {
            List<Node> reachableNodes = new List<Node>();
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();

            currentHero.CurrentNode.steps = 0;

            reachableNodes.Add(currentHero.CurrentNode);
            openSet.Add(currentHero.CurrentNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                int steps = currentNode.steps;
                steps++;

                if (steps <= currentHero.StepsCount)
                {
                    foreach (Node node in GetNeighbours(currentNode, steps))
                    {
                        if (!closedSet.Contains(node))
                        {
                            if (!openSet.Contains(node))
                            {
                                openSet.Add(node);
                                node.steps = steps;

                                if (node.steps <= currentHero.StepsCount)
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

                    if (x == z && steps == currentHero.StepsCount - 1)
                    {
                        continue;
                    }
                    if (x == -1 && z == 1 && steps == currentHero.StepsCount - 1)
                    {
                        continue;
                    }
                    if (x == 1 && z == -1 && steps == currentHero.StepsCount - 1)
                    {
                        continue;
                    }

                    int _x = x + currentNode.position.x;
                    int _y = currentNode.position.y;
                    int _z = z + currentNode.position.z;

                    if (_x == currentNode.position.x && _z == currentNode.position.z)
                        continue;

                    Node node = GridManager.instance.GetNode(_x, _y, _z, currentHero.CurrentNode.gridIndex, true);

                    if (_x == currentHero.CurrentNode.position.x &&
                                _z == currentHero.CurrentNode.position.z)
                    {
                        retVal.Add(currentHero.CurrentNode);
                    }
                    else if (node != null && node.IsWalkable())
                    {
                        retVal.Add(node);
                    }
                }
            }
            return retVal;
        }
        public void MoveHero(Node move)
        {
            currentHero.CurrentNode = move;

            heroesQueue.Remove(CurrentHero);
            heroesQueue.Add(CurrentHero);

            CurrentHero = heroesQueue.First();

            moveCount++;
        }
        public int Evaluate()
        {
            int distance = Mathf.Abs(Mathf.RoundToInt(Vector3.Distance(heroesQueue[0].CurrentNode.worldPosition, heroesQueue[1].CurrentNode.worldPosition)));

            if (distance == 1)
            {
                return -10000 + (moveCount - initialMoveCount);
            }

            return -distance;
        }

        public bool IsGameOver()
        {
            if (Vector3.Distance(heroesQueue[0].CurrentNode.worldPosition, heroesQueue[1].CurrentNode.worldPosition) <= 1)
                return true;
            return false;
        }
    }
}