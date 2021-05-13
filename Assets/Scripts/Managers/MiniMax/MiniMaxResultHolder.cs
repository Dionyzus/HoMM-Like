using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class MiniMaxResultHolder
    {
        UnitMove unitMove;
        int evaluationScore;

        public UnitMove UnitMove { get => unitMove; set => unitMove = value; }
        public int EvaluationScore { get => evaluationScore; set => evaluationScore = value; }

        public MiniMaxResultHolder()
        {

        }
        public MiniMaxResultHolder(UnitMove unitMove, int evaluationScore)
        {
            this.unitMove = unitMove;
            this.evaluationScore = evaluationScore;
        }
    }
}