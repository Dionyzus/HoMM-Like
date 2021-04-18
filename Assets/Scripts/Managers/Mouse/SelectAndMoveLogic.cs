using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Mouse Logic/Select and Move logic")]
    public class SelectAndMoveLogic : MouseLogic
    {
        public override void InteractTick(GameManager gameManager, RaycastHit hit)
        {
            InteractionHook ih = hit.transform.GetComponentInParent<InteractionHook>();
            if (ih)
                return;

            ISelectable selectable = hit.transform.GetComponentInParent<ISelectable>();

            if (selectable != null && selectable.GetGridUnit().gameObject.layer != GridManager.enemyUnitsLayer)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (selectable.GetGridUnit() != gameManager.targetUnit)
                    {
                        gameManager.pathLine.positionCount = 0;
                        gameManager.targetUnit = selectable.GetGridUnit();
                        UiManager.instance.OnCharacterSelected(gameManager.targetUnit);
                        gameManager.calculatePath = true;
                    }
                }
            }
            else
            {
                GameManager.instance.HandleMovingAction(hit.point);
            }
        }

        public override void InteractTick(GameManager gameManager, GridUnit gridUnit)
        {
            ISelectable selectable = gridUnit.GetComponentInParent<ISelectable>();

            if (selectable != null)
            {
                if (selectable.GetGridUnit() != gameManager.targetUnit)
                {
                    gameManager.pathLine.positionCount = 0;
                    gameManager.targetUnit = selectable.GetGridUnit();
                    gameManager.targetUnit.LoadPathAndStartMoving(gameManager.previousPath);
                }
                gameManager.calculatePath = true;
            }
        }
    }
}