using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Action Logic/Interaction with Scene Trigger")]
    public class InteractWithSceneTrigger : ActionLogic
    {
        [SerializeField]
        string targetScene = "";
        [SerializeField]
        UnitItem unitItem = default;
        [SerializeField]
        int stackSize = 0;
        public override void LoadAction(GridUnit gridUnit)
        {
            if (gridUnit != null)
            {
                InteractionHook ih = gridUnit.currentInteractionHook;
                if (ih != null)
                {
                    ih.interaction = new SceneTriggerInteraction(targetScene, unitItem, stackSize);
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