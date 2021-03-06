using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Action Logic/Interaction with World Object")]
    public class InteractWithWorldObject : ActionLogic
    {
        public override void LoadAction(GridUnit gridUnit)
        {
            if (gridUnit != null)
            {
                InteractionHook ih = gridUnit.currentInteractionHook;
                if (ih != null)
                {
                    ih.interaction = new WorldInteraction();
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