using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class CombatEvent
    {
        UnitController initiator;
        UnitController receiver;

        public CombatEvent(UnitController initiator, UnitController receiver)
        {
            this.initiator = initiator;
            this.receiver = receiver;
        }

        public void OnDamageReceived()
        {
            receiver.HitPoints -= initiator.Damage;

            if (receiver.HitPoints <= 0)
            {
                OnUnitDeath(receiver);
                BattleManager.instance.unitReceivedHitDebug = true;
                BattleManager.instance.UnitDeathCallback(receiver);
            }
            else
            {
                BattleManager.instance.unitReceivedHitDebug = true;
                receiver.LoadInteraction(new AttackReceived(initiator, receiver.takingHitAnimationClip, "Taking Hit"));
            }
        }

        void OnUnitDeath(UnitController unit)
        {
            unit.enabled = false;

            unit.Animator.applyRootMotion = true;
            unit.Animator.Play("Dying");
            unit.Animator.SetBool("isInteracting", true);
            unit.UnitCollider.enabled = false;
            unit.GetComponentInChildren<InteractionHook>().enabled = false;

            Collider[] colliders = unit.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
            {
                c.gameObject.layer = 9;
            }
            unit.Animator.transform.parent = null;

            if (unit.onDeathEnableCollider != null)
            {
                unit.onDeathEnableCollider.SetActive(true);
            }
        }
    }
}