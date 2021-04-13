using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Mouse Logic/Select and Move logic")]
    public class SelectAndMoveLogic : MouseLogic
    {
        public override void InteractTick(GameManager gameManager, RaycastHit hit)
        {
            if (hit.transform.GetComponentInParent<InteractionHook>() != null)
            {
                return;
            }
            ISelectable selectable = hit.transform.GetComponentInParent<ISelectable>();
            if (selectable != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (selectable.GetGridUnit() != gameManager.targetUnit)
                    {
                        gameManager.targetUnit = selectable.GetGridUnit();
                        gameManager.calculatePath = true;
                    }
                }
            }
            else
            {
                if (gameManager.targetUnit != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (gameManager.previousPath.Count > 0)
                        {
                            gameManager.ClearHighlightedNodes();
                            gameManager.targetUnit.LoadPathAndStartMoving(gameManager.previousPath);
                            gameManager.unitIsMoving = true;
                        }
                    }
                    Node currentNode = GridManager.instance.GetNode(hit.point, gameManager.targetUnit.gridIndex);
                    if (currentNode != null)
                    {
                        if (gameManager.reachableNodes.Contains(currentNode))
                        {
                            if (currentNode.IsWalkable())
                            {
                                if (gameManager.previousNode != currentNode)
                                {
                                    gameManager.HighlightNodes(currentNode);
                                    gameManager.GetPathFromMap(currentNode, gameManager.targetUnit);
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void InteractTick(GameManager gameManager, GridUnit gridUnit)
        {
            ISelectable selectable = gridUnit.GetComponentInParent<ISelectable>();
            if (selectable != null)
            {
                if (selectable.GetGridUnit() != gameManager.targetUnit)
                {
                    gameManager.targetUnit = selectable.GetGridUnit();
                    gameManager.targetUnit.LoadPathAndStartMoving(gameManager.previousPath);
                    gameManager.calculatePath = true;
                }
                else
                {
                    gameManager.calculatePath = true;
                }

            }
        }
    }
}