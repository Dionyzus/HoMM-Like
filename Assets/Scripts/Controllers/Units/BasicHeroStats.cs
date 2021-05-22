using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.ObjectModel;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Hero/Basic Hero Stats")]
	public class BasicHeroStats : ScriptableObject
    {
        //Grid specific
        public int gridIndex;
        public int stepsCount;

        //Movement specific
        public float movementSpeed;
        public float rotationSpeed;
	}
}