using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class HeroMove
    {
        private Node node;
        private int moveEvaluation = 1;
        private SimulationHero heroAvailableFromNode;
        private bool isAttackMove;
        public Node TargetNode { get => node; set => node = value; }
        public int MoveEvaluation { get => moveEvaluation; set => moveEvaluation = value; }
        public SimulationHero HeroAvailableFromNode { get => heroAvailableFromNode; set => heroAvailableFromNode = value; }
        public bool IsAttackMove { get => isAttackMove; set => isAttackMove = value; }
    }
}