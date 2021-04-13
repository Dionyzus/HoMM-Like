using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HOMM_BM
{
    public class AnimatorEvents : MonoBehaviour
    {
        public UnityEvent onFinish;

        public void OnFinish()
        {
            onFinish.Invoke();
        }
    }
}