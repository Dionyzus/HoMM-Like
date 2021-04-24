using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class AttackReceived : Interaction
    {
        float timer;

        UnitController initiator;
        AnimationClip animationClip;
        string animation;
        public AttackReceived(UnitController initiator, AnimationClip animationClip, string animation)
        {
            this.initiator = initiator;
            this.animationClip = animationClip;
            this.animation = animation;
        }
        public override void OnEnd(GridUnit gridUnit)
        {
            UnitController unitController = (UnitController)gridUnit;
            unitController.HitReceivedCompleted();
        }

        public override bool TickIsFinished(GridUnit gridUnit, float deltaTime)
        {
            timer -= deltaTime;
            Vector3 direction = (initiator.transform.position - gridUnit.transform.position).normalized;
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
            timer = animationClip.length;
            gridUnit.PlayAnimation(animation);
            gridUnit.Animator.SetBool("isInteracting", true);
        }
    }
}