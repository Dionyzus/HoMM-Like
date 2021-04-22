using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Mouse Logic/Target Node for Action")]
    public class TargetNodeForAction : MouseLogicBattle
    {
        public override void InteractTick(BattleManager battleManager, RaycastHit hit)
        {
            if (battleManager.currentUnit != null)
            {
                Node currentNode = GridManager.instance.GetNode(hit.point, battleManager.currentUnit.gridIndex);
                if (currentNode != null)
                {
                    battleManager.currentUnit.currentGridAction.Tick(currentNode);

                    if (Input.GetMouseButtonDown(0))
                    {
                        battleManager.currentUnit.currentGridAction.OnDoAction(battleManager.currentUnit);
                    }
                }
            }
        }
        public override void InteractTick(BattleManager battleManager, UnitController unitController)
        {
            if (battleManager.currentUnit != null)
            {
                Node currentNode = GridManager.instance.GetNode(unitController.CurrentNode.worldPosition, battleManager.currentUnit.gridIndex);
                if (currentNode != null)
                {
                    battleManager.currentUnit.currentGridAction.Tick(currentNode);

                    if (Input.GetMouseButtonDown(0))
                    {
                        battleManager.currentUnit.currentGridAction.OnDoAction(battleManager.currentUnit);
                    }
                }
            }
        }
    }
}