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
                    gameManager.targetUnit.currentGridAction.Tick(currentNode);

                    if (Input.GetMouseButtonDown(0))
                    {
                        gameManager.targetUnit.currentGridAction.OnDoAction(gameManager.targetUnit);
                    }
                }
            }
        }
        public override void InteractTick(GameManager gameManager, GridUnit gridUnit)
        {
            if (gameManager.targetUnit != null)
            {
                Node currentNode = GridManager.instance.GetNode(gridUnit.CurrentNode.worldPosition, gameManager.targetUnit.gridIndex);
                if (currentNode != null)
                {
                    gameManager.targetUnit.currentGridAction.Tick(currentNode);

                    if (Input.GetMouseButtonDown(0))
                    {
                        gameManager.targetUnit.currentGridAction.OnDoAction(gameManager.targetUnit);
                    }
                }
            }
        }
    }
}