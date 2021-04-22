using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [System.Serializable]
    public class GridAction
    {
        public string actionAnimation;
        public AnimationClip animationClip;
        List<Node> path = new List<Node>();

        public void Tick(Node currentNode)
        {
            path.Clear();

            path.Add(currentNode);
            FlowmapPathfinderMaster.instance.LoadNodesToPath(path);
        }

        public void OnDoAction(UnitController unit)
        {
            unit.LoadGridActionToMove(path, animationClip);
            unit.PlayAnimation(actionAnimation);
            GameManager.BattleManager.unitIsMoving = true;
        }
    }
}