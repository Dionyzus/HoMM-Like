using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class UnitMove
    {
        private Node node;
        private int moveEvaluation = 1;
        private SimpleUnit targetAvailableFromNode;
        private bool isAttackMove;

        public Node TargetNode { get => node; set => node = value; }
        public int MoveEvaluation { get => moveEvaluation; set => moveEvaluation = value; }
        public SimpleUnit TargetAvailableFromNode { get => targetAvailableFromNode; set => targetAvailableFromNode = value; }
        public bool IsAttackMove { get => isAttackMove; set => isAttackMove = value; }
    }
}