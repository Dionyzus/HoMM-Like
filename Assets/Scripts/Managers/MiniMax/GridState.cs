using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HOMM_BM
{
    public class GridState
    {
        int MAX = 1000;
        int MIN = -1000;

        //UnitController currentUnit;
        SimpleUnit currentSimple;

        //UnitController debugTarget;
        SimpleUnit debugSimple;

        //List<UnitController> humanUnits = new List<UnitController>();
        //List<UnitController> aiUnits = new List<UnitController>();

        List<SimpleUnit> humanSimples= new List<SimpleUnit>();
        List<SimpleUnit> aiSimples = new List<SimpleUnit>();

        List<SimpleUnit> unitsQueue = new List<SimpleUnit>();

        //public List<UnitController> HumanUnits { get => humanUnits; set => humanUnits = value; }
        //public List<UnitController> AiUnits { get => aiUnits; set => aiUnits = value; }
        //public UnitController DebugTarget { get => debugTarget; set => debugTarget = value; }
        //public UnitController CurrentUnit { get => currentUnit; set => currentUnit = value; }
        public List<SimpleUnit> HumanSimples { get => humanSimples; set => humanSimples = value; }
        public List<SimpleUnit> AiSimples { get => aiSimples; set => aiSimples = value; }
        public SimpleUnit CurrentSimple { get => currentSimple; set => currentSimple = value; }
        public SimpleUnit DebugSimple { get => debugSimple; set => debugSimple = value; }
        public List<SimpleUnit> UnitsQueue { get => unitsQueue; set => unitsQueue = value; }

        public GridState(List<SimpleUnit> unitsQueue)
        {
            this.unitsQueue = new List<SimpleUnit>();

            foreach(SimpleUnit unit in unitsQueue)
            {
                SimpleUnit newSimpleUnit = new SimpleUnit(unit.UnitStats, unit.CurrentNode, unit.Layer);
                this.unitsQueue.Add(newSimpleUnit);
            }

            currentSimple = this.unitsQueue.First();

            Initialize();
        }

        public void MoveUnit(Node targetNode)
        {
            currentSimple.CurrentNode = targetNode;

            //if (debugSimple != null)
            //{
            //    debugSimple.HitPoints -= currentSimple.Damage;
            //    if (debugSimple.UnitStats.hitPoints <= 0)
            //    {
            //        humanSimples.Remove(debugSimple);
            //    }
            //}

            unitsQueue.Remove(currentSimple);
            unitsQueue.Add(currentSimple);

            currentSimple = unitsQueue.First();

            ReadGridUnits();
        }
        public void Initialize()
        {
            ReadGridUnits();
        }

        void ReadGridUnits()
        {
            UnitController[] gridUnits = Object.FindObjectsOfType<UnitController>();

            foreach (UnitController unit in gridUnits)
            {
                if (!unit.enabled)
                    continue;

                if (unit.gameObject.layer == GridManager.ENEMY_UNITS_LAYER)
                {
                    //aiUnits.Add(unit);
                    SimpleUnit simpleUnit = new SimpleUnit(unit.unitStats, unit.CurrentNode, unit.gameObject.layer);
                    aiSimples.Add(simpleUnit);
                }
                else
                {
                    //humanUnits.Add(unit);
                    SimpleUnit simpleUnit = new SimpleUnit(unit.unitStats, unit.CurrentNode, unit.gameObject.layer);
                    humanSimples.Add(simpleUnit);
                }
            }
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

        public List<Node> UpdateUnitMoves()
        {
            List<Node> reachableNodes = CalculateWalkablePositions();
            EvaluateMoves(reachableNodes);

            return reachableNodes;
        }

        void EvaluateMoves(List<Node> reachableNodes)
        {
            List<SimpleUnit> possibleTargets = new List<SimpleUnit>();

            UnitController[] gridUnits = Object.FindObjectsOfType<UnitController>();

            foreach (UnitController unit in gridUnits)
            {
                if (unit.gameObject.layer == currentSimple.Layer)
                    continue;
                else
                {
                    SimpleUnit newSimpleUnit = new SimpleUnit(unit.unitStats, unit.CurrentNode, unit.gameObject.layer);
                    possibleTargets.Add(newSimpleUnit);
                }
            }

            float minDistance = float.MaxValue;

            Node closestNode = null;
            SimpleUnit closestTarget = null;

            for (int i = 0; i < reachableNodes.Count; i++)
            {
                float distance = float.MaxValue;

                foreach (SimpleUnit unit in possibleTargets)
                {
                    float distanceToClosestTarget = Vector3.Distance(reachableNodes[i].position, unit.CurrentNode.position);
                    if (distanceToClosestTarget < distance)
                    {
                        distance = distanceToClosestTarget;
                        closestTarget = unit;
                    }
                }

                if (IsTargetNodeNeighbour(reachableNodes[i], closestTarget.CurrentNode))
                {
                    debugSimple = closestTarget;
                    reachableNodes[i].score = 10;
                }

                if (distance < minDistance && reachableNodes[i].IsWalkable())
                {
                    minDistance = distance;
                    closestNode = reachableNodes[i];
                }
            }

            closestNode.score = 5;
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

        public int Evaluate()
        {
            int maxTotalHitPoints = CalculateHitPoints(aiSimples);
            int minTotalHitPoints = CalculateHitPoints(humanSimples);

            if (maxTotalHitPoints == 0)
            {
                return MIN;
            }
            else if (minTotalHitPoints == 0)
            {
                return MAX;
            }

            //return maxTotalHitPoints - minTotalHitPoints;
            return Mathf.FloorToInt(Vector3.Distance(unitsQueue.ElementAt(0).CurrentNode.position, unitsQueue.ElementAt(1).CurrentNode.position) * 10);
        }

        public bool IsGameOver()
        {
            if (aiSimples.Count == 0 || humanSimples.Count == 0)
            {
                Debug.Log("Game Over");
                return true;
            }
            return false;
        }

        int CalculateHitPoints(List<SimpleUnit> units)
        {
            int retVal = 0;

            foreach (SimpleUnit unit in units)
            {
                retVal += unit.HitPoints;
            }

            return retVal;
        }
    }
}