using UnityEngine;

namespace HOMM_BM
{
    public class InteractionHook : GridObject
    {
        public InteractionContainer interactionContainer;
        public Interaction interaction;
        public Transform interactionPoint;

        public void LoadInteraction(GridUnit gridUnit)
        {
            if (interaction != null)
                gridUnit.LoadInteraction(interaction);
        }
    }
}