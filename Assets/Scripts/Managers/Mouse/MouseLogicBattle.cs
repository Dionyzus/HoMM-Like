using UnityEngine;

namespace HOMM_BM
{
    public abstract class MouseLogicBattle : ScriptableObject
    {
        public abstract void InteractTick(BattleManager battleManager, RaycastHit hit);
    }
}