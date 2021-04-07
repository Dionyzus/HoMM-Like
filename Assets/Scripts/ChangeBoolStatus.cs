using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class ChangeBoolStatus : StateMachineBehaviour
    {
        [SerializeField]
        StringBoolDictionary animationStatus = new StringBoolDictionary();
        public IDictionary<string, bool> AnimationStatus
        {
            get { return animationStatus; }
            set { animationStatus.CopyFrom(value); }
        }

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            SetBools(animator, animationStatus);
        }
        void SetBools(Animator animator, StringBoolDictionary animationStatus)
        {
            foreach (KeyValuePair<string, bool> entry in animationStatus)
            {
                animator.SetBool(entry.Key, entry.Value);
            }
        }
    }
}
