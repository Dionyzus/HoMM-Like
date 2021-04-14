using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class WaitForTimerToFinish : Interaction
    {
        float timer = 0.5f;
        public override void OnEnd(GridUnit gridUnit)
        {
            gridUnit.ActionIsDone();
        }

        public override bool TickIsFinished(GridUnit gridUnit, float deltaTime)
        {
            timer -= deltaTime;

            Vector3 direction = (gridUnit.currentInteractionHook.transform.position - gridUnit.transform.position).normalized;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            gridUnit.transform.rotation = Quaternion.Slerp(gridUnit.transform.rotation, rotation, deltaTime / .3f);

            if (timer < 0)
            {
                return true;
            }
            return false;
        }

        protected override void OnStart(GridUnit gridUnit)
        {
            Debug.Log("Interaction started");
        }
    }
}