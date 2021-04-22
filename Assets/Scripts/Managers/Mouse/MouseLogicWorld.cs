using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public abstract class MouseLogicWorld : ScriptableObject
    {
        public abstract void InteractTick(WorldManager worldManager, RaycastHit hit);
        public abstract void InteractTick(WorldManager worldManager, HeroController heroController);
    }
}