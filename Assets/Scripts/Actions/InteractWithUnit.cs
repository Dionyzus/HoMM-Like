using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Action Logic/Interaction with Unit")]
    public class InteractWithUnit : ActionLogic
    {
        public override void LoadAction(GridUnit gridUnit)
        {
            if (gridUnit != null)
            {
                InteractionHook ih = gridUnit.currentInteractionHook;
                if (ih != null)
                {
                    //Basically always the same, since we just want to store interactionHook
                    ih.interaction = new InitiateAttack();
                    gridUnit.StoreInteractionHook(ih);
                    return;
                }
                else
                {
                    Debug.Log("Ih is null!");
                }
            }
        }
        public override void ActionDone(GridUnit gridUnit)
        {
            gridUnit.InteractionCompleted();
        }
    }
}