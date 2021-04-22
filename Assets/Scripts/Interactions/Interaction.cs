using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public abstract class Interaction
    {
        bool initialized;
        public void StartMethod(GridUnit gridUnit)
        {
            if (!initialized)
            {
                OnStart(gridUnit);
                initialized = true;
            }
        }
        protected abstract void OnStart(GridUnit gridUnit);
        public abstract void OnEnd(GridUnit gridUnit);
        public abstract bool TickIsFinished(GridUnit gridUnit, float deltaTime);
    }
}