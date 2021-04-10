using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Mouse Logic/Target Node for Action")]
    public class TargetNodeForAction : MouseLogic
    {
        public override void InteractTick(GameManager gameManager, RaycastHit hit)
        {
            if (gameManager.targetUnit != null)
            {
                Node currentNode = GridManager.instance.GetNode(hit.point, gameManager.targetUnit.gridIndex);
                if (currentNode != null)
                {
                    gameManager.targetUnit.currentGridAction.Tick(currentNode, gameManager.targetUnit);

                    if (Input.GetMouseButtonDown(0))
                    {
                        gameManager.targetUnit.currentGridAction.OnDoAction(gameManager.targetUnit);
                    }
                }
            }
        }
    }
}