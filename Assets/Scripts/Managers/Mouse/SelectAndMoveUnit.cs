using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Mouse Logic/Select and Move Unit")]
    public class SelectAndMoveUnit : MouseLogicBattle
    {
        public override void InteractTick(BattleManager battleManager, RaycastHit hit)
        {
            InteractionHook ih = hit.transform.GetComponentInParent<InteractionHook>();
            if (ih)
                return;

            ISelectable selectable = hit.transform.GetComponentInParent<ISelectable>();

            if (selectable != null && selectable.GetGridUnit().gameObject.layer != GridManager.enemyUnitsLayer)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (selectable.GetGridUnit() != battleManager.currentUnit && selectable.GetType() == typeof(UnitController))
                    {
                        FlowmapPathfinderMaster.instance.pathLine.positionCount = 0;
                        battleManager.currentUnit = (UnitController)selectable.GetGridUnit();
                        UiManager.instance.OnUnitTurn(battleManager.currentUnit);
                        battleManager.calculatePath = true;
                    }
                }
            }
            else
            {
                battleManager.HandleMovingAction(hit.point);
            }
        }

        public override void InteractTick(BattleManager battleManager, UnitController unitController)
        {
            ISelectable selectable = unitController.GetComponentInParent<ISelectable>();

            if (selectable != null)
            {
                if (selectable.GetGridUnit() != battleManager.currentUnit)
                {
                    FlowmapPathfinderMaster.instance.pathLine.positionCount = 0;
                    battleManager.currentUnit = (UnitController)selectable.GetGridUnit();
                    battleManager.currentUnit.LoadPathAndStartMoving(FlowmapPathfinderMaster.instance.previousPath);
                }
                battleManager.calculatePath = true;
            }
        }
    }
}