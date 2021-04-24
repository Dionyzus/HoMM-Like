using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Action Logic/Move To Location")]
    public class MoveToLocation : ActionLogic
    {
        public override void LoadAction(GridUnit gridUnit)
        {
            if (gridUnit != null)
            {
                //Could create default interaction then interaction itself could be one determinating
                //how unit will move etc.
                gridUnit.MoveToLocation();
            }
        }
        public override void ActionDone(GridUnit gridUnit)
        {
            gridUnit.MovingToLocationCompleted();
        }
    }
}