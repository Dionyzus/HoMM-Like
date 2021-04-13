using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public abstract class ActionLogic : ScriptableObject
    {
        public abstract void LoadAction(GridUnit gridUnit);
        public abstract void ActionDone(GridUnit gridUnit);
    }
}