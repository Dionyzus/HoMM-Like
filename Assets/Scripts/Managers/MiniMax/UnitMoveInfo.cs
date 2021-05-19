using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class UnitMoveInfo
    {
        SimpleUnit unit;
        UnitMove unitMove;
        float distance;

        public UnitMoveInfo(SimpleUnit unit, UnitMove unitMove, float distance)
        {
            this.unit = unit;
            this.unitMove = unitMove;
            this.distance = distance;
        }

        public SimpleUnit Unit { get => unit; set => unit = value; }
        public UnitMove UnitMove { get => unitMove; set => unitMove = value; }
        public float Distance { get => distance; set => distance = value; }
    }
}