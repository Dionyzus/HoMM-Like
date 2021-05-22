using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class HeroStatsReference : MonoBehaviour
    {
        int gridIndex;
        int stepsCount;

        float movementSpeed;
        float rotationSpeed;

        public int GridIndex { get => gridIndex; set => gridIndex = value; }
        public int StepsCount { get => stepsCount; set => stepsCount = value; }
        public float MovementSpeed { get => movementSpeed; set => movementSpeed = value; }
        public float RotationSpeed { get => rotationSpeed; set => rotationSpeed = value; }

        public HeroStatsReference(int gridIndex, int stepsCount, float movementSpeed, float rotationSpeed)
        {
            this.gridIndex = gridIndex;
            this.stepsCount = stepsCount;
            this.movementSpeed = movementSpeed;
            this.rotationSpeed = rotationSpeed;
        }
    }
}