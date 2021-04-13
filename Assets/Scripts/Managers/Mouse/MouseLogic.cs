using UnityEngine;

namespace HOMM_BM
{
    public abstract class MouseLogic : ScriptableObject
    {
        public abstract void InteractTick(GameManager gameManager, RaycastHit hit);
        public abstract void InteractTick(GameManager gameManager, GridUnit gridUnit);
    }
}