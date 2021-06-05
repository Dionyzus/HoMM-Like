using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOMM_BM
{
    public class AnimatorEvents : MonoBehaviour
    {
        public UnityEvent onFinish;
        public UnityEvent onAnimationHit;
        public UnityEvent onProjectileFired;
        public UnityEvent onStep;

        public void OnFinish()
        {
            onFinish.Invoke();
        }

        public void OnProjectileFired()
        {
            onProjectileFired.Invoke();
        }

        public void OnStep()
        {
            onStep.Invoke();
        }

        public void OnAnimationHit()
        {
            BattleManager.instance.CurrentCombatEvent.OnDamageReceived();
        }
    }
}