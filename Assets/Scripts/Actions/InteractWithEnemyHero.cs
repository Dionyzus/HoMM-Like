using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Action Logic/Interaction with Enemy Hero")]
    public class InteractWithEnemyHero : ActionLogic
    {
        [SerializeField]
        HeroInteractionHook heroInteractioHook;
        [SerializeField]
        string targetScene = "";

        public HeroInteractionHook HeroInteractioHook { get => heroInteractioHook; set => heroInteractioHook = value; }

        public override void LoadAction(GridUnit gridUnit)
        {
            if (gridUnit != null)
            {
                InteractionHook ih = gridUnit.currentInteractionHook;
                if (ih != null)
                {
                    ih.interaction = new InteractWithHeroInteraction(targetScene, heroInteractioHook);
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