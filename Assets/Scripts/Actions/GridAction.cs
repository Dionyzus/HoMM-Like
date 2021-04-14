﻿using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [System.Serializable]
    public class GridAction
    {
        public string playAnimation;
        public AnimationClip animationClip;
        List<Node> path = new List<Node>();

        public void Tick(Node currentNode)
        {
            path.Clear();

            path.Add(currentNode);
            GameManager.instance.LoadNodesToPath(path);
        }

        public void OnDoAction(GridUnit unit)
        {
            unit.LoadGridActionToMove(path, animationClip);
            unit.PlayAnimation(playAnimation);
            GameManager.instance.unitIsMoving = true;
        }
    }
}