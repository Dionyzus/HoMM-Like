using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class InteractionTrigger : MonoBehaviour
    {
        public Transform lookTarget;
        public GameObject interactionParent;
        private void OnTriggerEnter(Collider collider)
        {
            GridUnit unit = collider.GetComponentInParent<GridUnit>();
            if (unit != null && unit.currentInteractionHook != null)
            {
                //Should probably add wait for interaction logic
                Vector3 direction = lookTarget.position - unit.CurrentNode.worldPosition;
                direction.y = 0;
                if (direction == Vector3.zero)
                {
                    direction = unit.transform.forward;
                }

                unit.transform.rotation = Quaternion.LookRotation(direction);
                GameManager.instance.calculatePath = true;
                //Add some logic, like add resources, buff stat, start some interaction
                //For now just "picking the item", aka destroying this interaction object.
                Destroy(interactionParent);
            }
        }
    }
}