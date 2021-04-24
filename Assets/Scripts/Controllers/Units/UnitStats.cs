using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Unit/Unit Stats")]
    public class UnitStats : ScriptableObject
    {
        //Grid specific
        public int stepsCount;
        public int gridIndex;

        //Movement specific
        public float movementSpeed;
        public float rotationSpeed;

        //Combat specific
        public float initiative;
        public int hitPoints;
        public int hitDamage;
    }
}