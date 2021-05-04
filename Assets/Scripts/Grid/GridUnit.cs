using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class GridUnit : MonoBehaviour, ISelectable
    {
        public delegate void OnPathReachCallback();
        protected OnPathReachCallback onPathReachCallback;

        protected Quaternion targetRotation;
        protected float moveTime;
        protected float rotationTime;

        protected int gridIndex = 0;
        public int GridIndex { get => gridIndex; set => gridIndex = value; }

        protected float movementSpeed;
        protected float rotationSpeed;

        protected int stepsCount = 3;
        public int StepsCount { get => stepsCount; set => stepsCount = value; }

        //Will be used when floors are added
        protected int verticalStepsUp = 1;
        protected int verticalStepsDown = 3;

        public Node CurrentNode
        {
            get
            {
                return GridManager.instance.GetNode(transform.position, gridIndex);
            }
        }

        public string actionAnimation;
        public AnimationClip animationClip;

        protected float time;
        protected int index;
        protected List<Node> currentPath = new List<Node>();

        protected bool isPathInitialized;

        protected Vector3 originPosition;
        protected Vector3 targetPosition;

        protected float actualMovementSpeed;
        protected bool isWalking;
        protected bool isDirty;

        protected bool isInteractionInitialized;
        public bool IsInteractionInitialized { get => isInteractionInitialized; set => isInteractionInitialized = value; }

        protected bool moveIsBasedOnAnimation;
        protected float animationLength;
        public bool IsInteracting
        {
            get
            {
                return animator.GetBool("isInteracting");
            }
        }

        protected Animator animator;
        public Animator Animator
        {
            get
            {
                return animator;
            }
        }

        public bool IsWalking { get => isWalking; set => isWalking = value; }

        public AnimatorOverrideController overrideController;
        private Collider unitCollider;
        public Collider UnitCollider { get => unitCollider; set => unitCollider = value; }

        [HideInInspector]
        public InteractionHook currentInteractionHook;

        public InteractionInstance currentInteractionInstance;
        public InteractionInstance interactionInstance;

        public Interaction currentInteraction;


        void Awake()
        {
            //Maybe seperate enemy and friendly units
            //gameObject.layer = 8;
        }

        void Start()
        {
            animator = GetComponentInChildren<Animator>();
            if (overrideController != null)
                animator.runtimeAnimatorController = overrideController;

            animator.applyRootMotion = false;
            isInteractionInitialized = false;

            unitCollider = GetComponentInChildren<Collider>();
        }
        void Update()
        {
            //Check this out...
            //animator.applyRootMotion = IsInteracting;
            animator.SetBool("isWalking", IsWalking);

            if (IsInteracting)
            {
                Vector3 deltaPosition = animator.deltaPosition;
                transform.position += deltaPosition;

                animator.transform.localPosition = Vector3.zero;
                if (!isDirty)
                    isDirty = true;
            }
            else
            {
                if (isDirty)
                {
                    isDirty = false;
                    transform.position = CurrentNode.worldPosition;
                }
            }
        }
        public void PlayAnimation(string animation, bool applyRootMotion = false)
        {
            animator.applyRootMotion = applyRootMotion;
            animator.Play(animation);
        }
        public virtual void CreateInteractionContainer(InteractionContainer container)
        {
            Debug.Log("Default create interaction container");
        }
        public virtual void StoreInteractionHook(InteractionHook interactionHook)
        {
            Debug.Log("Default load interaction from hook and store");
        }
        public virtual void LoadInteraction(Interaction targetInteraction)
        {
            Debug.Log("Default load interaction");
        }
        protected virtual void HandleInteraction(Interaction interaction, float deltaTime)
        {
            Debug.Log("Default handle interaction");
        }
        public virtual void ActionIsDone()
        {
            Debug.Log("Default action is done");
        }
        public virtual void LoadActionFromInteractionContainer(InteractionInstance instance)
        {
            Debug.Log("Default load action from interaction container");
        }
        public virtual void InteractionCompleted()
        {
            Debug.Log("Default interaction completed");
        }
        public GridUnit GetGridUnit()
        {
            return this;
        }

        public virtual void InitializeMoveToInteractionContainer(Node targetNode)
        {
            Debug.Log("Default load to interaction container");
        }
        public virtual void MoveToLocation()
        {
            Debug.Log("Default move to location");
        }
        public virtual void MovingToLocationCompleted()
        {
            Debug.Log("Default moving to location completed");
        }
    }
}