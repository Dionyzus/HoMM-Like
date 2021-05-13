using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class StatComparatorHolder
    {
        private float first;
        private float second;

        public float First { get => first; set => first = value; }
        public float Second { get => second; set => second = value; }

        public StatComparatorHolder(float first, float second)
        {
            this.first = first;
            this.second = second;
        }
    }
}