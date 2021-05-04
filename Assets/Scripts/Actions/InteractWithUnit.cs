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
                UnitController unitController = (UnitController)gridUnit;
                InteractionHook ih = gridUnit.currentInteractionHook;

                if (ih != null)
                {
                    //Basically always the same, since we just want to store interactionHook
                    if (unitController.AttackType.Equals(Enums.UnitAttackType.RANGED))
                    {
                        ih.interaction = new InitiateRangedAttack();
                    }
                    else if (unitController.AttackType.Equals(Enums.UnitAttackType.MELEE))
                    {
                        ih.interaction = new InitiateMeleeAttack();
                    }
                    else if(unitController.AttackType.Equals(Enums.UnitAttackType.MAGIC))
                    {
                        Debug.Log("Future magic attack");
                        //ih.interaction = new InitiateMagicAttack();
                    } else
                    {
                        Debug.Log("Missing attack type!");
                    }

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