using UnityEngine;

namespace HOMM_BM
{
    public class InteractionHook : GridObject
    {
        public Transform interactionPoint;
        public InteractionStack[] interactionStacks;

        public void LoadInteraction(GridUnit gridUnit)
        {
            gridUnit.LoadInteraction(new WaitForTimerToFinish());
        }
    }
}