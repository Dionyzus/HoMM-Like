using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Mouse Logic/Perform Attack Logic")]
    public class PerformAttackLogic : MouseLogic
    {
        public override void InteractTick(GameManager gameManager, RaycastHit hit)
        {
            InteractionHook hook = hit.transform.GetComponentInParent<InteractionHook>();

            if (gameManager.targetUnit != null)
            {
                if (hook != null)
                {
                    gameManager.targetUnit.currentInteractionHook = hook;
                }
                else
                {
                    gameManager.targetUnit.currentInteractionHook = null;
                }
            }
        }

        public override void InteractTick(GameManager gameManager, GridUnit gridUnit)
        {
            throw new System.NotImplementedException();
        }
    }
}