using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Hero/Hero Stats")]
    public class HeroStats : ScriptableObject
    {
        //Grid specific
        public int gridIndex;
        public int stepsCount;

        //Movement specific
        public float movementSpeed;
        public float rotationSpeed;
    }
}