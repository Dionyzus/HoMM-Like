using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class UnitMove
    {
        Node targetNode;
        UnitController unitController;
        UnitController targetUnit;
        SimpleUnit simpleUnit;
        SimpleUnit targetSimple;

        public Node TargetNode { get => targetNode; set => targetNode = value; }
        public UnitController UnitController { get => unitController; set => unitController = value; }
        public UnitController TargetUnit { get => targetUnit; set => targetUnit = value; }
        public SimpleUnit SimpleUnit { get => simpleUnit; set => simpleUnit = value; }
        public SimpleUnit TargetSimple { get => targetSimple; set => targetSimple = value; }

        public UnitMove(UnitController unitController, UnitController targetUnit, Node targetNode)
        {
            this.unitController = unitController;
            this.targetUnit = targetUnit;
            this.targetNode = targetNode;
        }

        public UnitMove(SimpleUnit simpleUnit, SimpleUnit targetSimple, Node targetNode)
        {
            this.simpleUnit = simpleUnit;
            this.targetSimple = targetSimple;
            this.targetNode = targetNode;
        }
    }
}
