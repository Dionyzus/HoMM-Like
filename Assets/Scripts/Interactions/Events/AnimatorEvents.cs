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

        public void OnFinish()
        {
            onFinish.Invoke();
        }

        public void OnAnimationHit()
        {
            //onAnimationHit.Invoke();
            BattleManager.instance.CurrentCombatEvent.OnDamageReceived();
        }
    }
}