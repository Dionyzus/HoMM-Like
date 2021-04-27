using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class InitiateAttack : Interaction
    {
        float timer;
        public override void OnEnd(GridUnit gridUnit)
        {
            gridUnit.ActionIsDone();
        }

        public override bool TickIsFinished(GridUnit gridUnit, float deltaTime)
        {
            timer -= deltaTime;

            RotateTargetUnit(gridUnit, deltaTime);

            Vector3 direction = (gridUnit.currentInteractionHook.transform.position - gridUnit.transform.position).normalized;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            gridUnit.transform.rotation = Quaternion.Slerp(gridUnit.transform.rotation, rotation, deltaTime / .3f);

            if (timer <= 0)
            {
                return true;
            }
            return false;
        }

        void RotateTargetUnit(GridUnit gridUnit, float deltaTime)
        {
            UnitController targetUnit = gridUnit.currentInteractionHook.GetComponentInParent<UnitController>();
            if (targetUnit != null)
            {
                Vector3 direction = (gridUnit.transform.position - targetUnit.transform.position).normalized;
                direction.y = 0;
                Quaternion rotation = Quaternion.LookRotation(direction);

                targetUnit.transform.rotation = Quaternion.Slerp(targetUnit.transform.rotation, rotation, deltaTime / .3f);
            }
        }

        protected override void OnStart(GridUnit gridUnit)
        {
            timer = gridUnit.animationClip.length;
            gridUnit.PlayAnimation(gridUnit.actionAnimation);
            gridUnit.Animator.SetBool("isInteracting", true);
        }
    }
}