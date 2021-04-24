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

            battleManager.HandleMovingAction(hit.point);
        }
    }
}