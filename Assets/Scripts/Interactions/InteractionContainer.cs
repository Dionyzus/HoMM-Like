using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Action Logic/Interaction Container")]
    public class InteractionContainer : ScriptableObject
    {
        public ActionLogic action;
        public Sprite moveDisplay;
        public Sprite interactDisplay;

        public void LoadAction(GridUnit gridUnit)
        {
            if (action != null)
            {
                action.LoadAction(gridUnit);
            }
            else
            {
                //Do something if can't perform action
            }
        }
    }

    public class InteractionInstance
    {
        public InteractionContainer interactionContainer;
        [System.NonSerialized]
        public InteractionButton uiObject;
        [System.NonSerialized]
        public GridUnit gridUnit;
    }
}