using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class InteractionButton : MonoBehaviour
    {
        public InteractionInstance interactionInstance;
        public Animator animator;

        public static InteractionButton instance;

        private void Awake()
        {
            instance = this;
        }
        public void OnClick()
        {
            SetToDestroy();
        }
        public void SetToDestroy()
        {
            animator.Play("Close");
        }
        public void SelfDestruct()
        {
            Destroy(this.gameObject);
        }
    }
}