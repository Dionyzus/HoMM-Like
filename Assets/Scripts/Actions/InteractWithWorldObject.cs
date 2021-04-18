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
                InteractionHook ih = GameManager.instance.targetUnit.currentInteractionHook;
                if (ih != null)
                {
                    gridUnit.LoadInteractionFromHookAndStore(ih);
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
            gridUnit.StackIsComplete();
        }
    }
}