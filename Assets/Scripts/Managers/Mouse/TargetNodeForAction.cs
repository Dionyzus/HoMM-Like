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

                    if (GameManager.instance.Mouse.leftButton.isPressed)
                    {
                        battleManager.currentUnit.currentGridAction.OnDoAction(battleManager.currentUnit);
                    }
                }
            }
        }
    }
}