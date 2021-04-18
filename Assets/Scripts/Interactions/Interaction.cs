using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public abstract class Interaction
    {
        bool initialized;
        public void StartMethod(GridUnit gridUnit, AnimationClip animationClip, string animation)
        {
            if (!initialized)
            {
                OnStart(gridUnit, animationClip, animation);
                initialized = true;
            }
        }
        protected abstract void OnStart(GridUnit gridUnit, AnimationClip animationClip, string animation);
        public abstract void OnEnd(GridUnit gridUnit);
        public abstract bool TickIsFinished(GridUnit gridUnit, float deltaTime);
    }
}