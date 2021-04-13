using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu]
    public class InteractionStack : ScriptableObject
    {
        public ActionLogic[] actions;

        public void LoadAction(GridUnit gridUnit, int index)
        {
            if (index < actions.Length)
            {
                actions[index].LoadAction(gridUnit);
            }
            else
            {
                gridUnit.StackIsComplete();
            }
        }
    }

    public class InteractionInstance
    {
        public InteractionStack interactionStack;
        [System.NonSerialized]
        public InteractionButton uiObject;
        [System.NonSerialized]
        public GridUnit gridUnit;
    }
}