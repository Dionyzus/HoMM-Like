using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class InteractionHook : MonoBehaviour
    {
        public InteractionType InteractionType;

        public InteractionContainer interactionContainer;
        public Interaction interaction;
        public Transform interactionPoint;

        public string interactionAnimation;
        public GameObject particles;
        public Animator animator;

        public int gridIndex = 0;
        public Node CurrentNode
        {
            get
            {
                return GridManager.instance.GetNode(transform.position, gridIndex);
            }
        }

        [SerializeField] Item item;
        [SerializeField] int amount;
        public Item Item { get => item; set => item = value; }
        public int Amount { get => amount; set => amount = value; }

        public void LoadInteraction(GridUnit gridUnit)
        {
            if (interaction != null)
                gridUnit.LoadInteraction(interaction);
        }

        public void PlayAnimation()
        {
            animator.Play(interactionAnimation);
        }
        public void DestroyParticles()
        {
            Destroy(particles.gameObject);
        }
    }
}