using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class UnitController : GridUnit
    {
        UnitSide unitSide;
        public GridAction currentGridAction;
        public GameObject onDeathEnableCollider;

        Image unitImage;
        public Image UnitImage { get => unitImage; set => unitImage = value; }

        public AnimationClip takingHitAnimationClip;

        public UnitStats unitStats;

        [SerializeField]
        UnitAttackType attackType;

        //Unit controller specific data
        int initiative;
        public int Initiative { get => initiative; set => initiative = value; }

        public int hitPoints;
        public int damage;
        int attack;
        int defense;

        private bool isTargetPointBlank;
        public bool IsTargetPointBlank { get => isTargetPointBlank; set => isTargetPointBlank = value; }
        public int HitPoints { get => hitPoints; set => hitPoints = value; }
        public int Damage { get => damage; set => damage = value; }
        public int Attack { get => attack; set => attack = value; }
        public int Defense { get => defense; set => defense = value; }
        public UnitAttackType AttackType { get => attackType; set => attackType = value; }
        public bool ProjectileHitTarget { get => projectileHitTarget; set => projectileHitTarget = value; }
        public float MaximumRange { get => maximumRange; set => maximumRange = value; }
        public int MeleeAttackDamage { get => meleeAttackDamage; set => meleeAttackDamage = value; }
        public UnitSide UnitSide { get => unitSide; set => unitSide = value; }

        //Ranged
        [SerializeField]
        float maximumRange = 10f;
        [SerializeField]
        int meleeAttackDamage;
        [SerializeField]
        GameObject projectilePrefab = default;
        [SerializeField]
        Transform projectileSpawnPoint = default;

        bool projectileHitTarget;

        private void Awake()
        {
            unitImage = GetComponentInChildren<Image>();

            gridIndex = unitStats.gridIndex;
            stepsCount = unitStats.stepsCount;

            movementSpeed = unitStats.movementSpeed;
            rotationSpeed = unitStats.rotationSpeed;

            initiative = unitStats.initiative;
            hitPoints = unitStats.hitPoints;
            damage = unitStats.damage;
            attack = unitStats.attack;
            defense = unitStats.defense;

            if (attackType.Equals(UnitAttackType.RANGED))
            {
                meleeAttackDamage = damage / 3;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            //Check this out...
            //animator.applyRootMotion = IsInteracting;
            animator.SetBool("isWalking", isWalking);

            if (isInteractionInitialized && isTargetPointBlank)
            {
                if (interactionInstance != null)
                {
                    LoadActionFromInteractionContainer(interactionInstance);
                    interactionInstance = null;
                }
                currentInteractionHook.LoadInteraction(this);
                isInteractionInitialized = false;
            }

            if (isInteractionInitialized && !isTargetPointBlank)
            {
                if (interactionInstance != null)
                {
                    LoadActionFromInteractionContainer(interactionInstance);
                    interactionInstance = null;
                }
                TryLoadingInteractionFromHook();
            }

            if (currentInteraction != null)
            {
                HandleInteraction(currentInteraction, deltaTime);
            }
        }

        public bool MovingOnPathFinished()
        {
            bool isFinished = false;

            if (!isPathInitialized)
            {
                originPosition = CurrentNode.worldPosition;
                targetPosition = currentPath[index].worldPosition;
                time = 0;

                float distance = Vector3.Distance(originPosition, targetPosition);
                actualMovementSpeed = movementSpeed / distance;

                if (moveIsBasedOnAnimation)
                {
                    actualMovementSpeed = (distance / animationLength) / distance; //0-1
                }

                Vector3 direction = targetPosition - originPosition;
                direction.y = 0;
                if (direction == Vector3.zero)
                {
                    direction = transform.forward;
                }
                transform.rotation = Quaternion.LookRotation(direction);

                isPathInitialized = true;
                isWalking = true;
            }

            if (!moveIsBasedOnAnimation)
            {
                time += Time.deltaTime * actualMovementSpeed;
            }
            else
            {
                if (animator.GetBool("isMoving"))
                {
                    time += Time.deltaTime * actualMovementSpeed;
                }
                else
                {
                    if (time > 0)
                    {
                        time = 1;
                    }
                }
            }

            if (time > 1)
            {
                isPathInitialized = false;

                index++;

                if (FlowmapPathfinderMaster.instance.pathLine.positionCount > 0)
                    FlowmapPathfinderMaster.instance.pathLine.positionCount -= 1;

                if (index > currentPath.Count - 1)
                {
                    time = 1;
                    isWalking = false;
                    moveIsBasedOnAnimation = false;
                    isFinished = true;
                }
            }

            transform.position = Vector3.Lerp(originPosition, targetPosition, time);

            return isFinished;
        }
        public void PerformRangedAttack()
        {
            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.transform.position, Quaternion.identity);

            RangedProjectile rangedProjectile = projectileInstance.GetComponent<RangedProjectile>();

            if (rangedProjectile != null)
            {
                rangedProjectile.TargetUnit = currentInteractionHook.GetComponentInParent<UnitController>();
                rangedProjectile.IsTargetSet = true;

                projectileInstance.SetActive(true);
            }
        }

        public void LoadPathAndStartMoving(List<Node> path, bool reverse = true)
        {
            moveIsBasedOnAnimation = false;
            isPathInitialized = false;
            index = 0;
            time = 0;

            if (reverse)
                path.Reverse();
            currentPath = path;
        }
        public void LoadGridActionToMove(List<Node> path, AnimationClip animationClip)
        {
            isPathInitialized = false;
            index = 0;
            time = 0;
            moveIsBasedOnAnimation = true;
            animationLength = animationClip.length;
            currentPath = path;
        }
        public override void CreateInteractionContainer(InteractionContainer container)
        {
            InteractionInstance ii = new InteractionInstance
            {
                interactionContainer = container,
                gridUnit = this
            };
            interactionInstance = ii;

            UiManager.instance.CreateUiObjectForInteraction(ii);
        }
        public override void StoreInteractionHook(InteractionHook interactionHook)
        {
            currentInteractionHook = interactionHook;
        }
        void TryLoadingInteractionFromHook()
        {
            if (!BattleManager.instance.unitIsMoving)
            {
                currentInteractionHook.LoadInteraction(this);
                isInteractionInitialized = false;
            }
        }
        public override void LoadInteraction(Interaction targetInteraction)
        {
            currentInteraction = targetInteraction;
        }
        protected override void HandleInteraction(Interaction interaction, float deltaTime)
        {
            if (currentInteractionHook && attackType.Equals(UnitAttackType.MELEE))
                BattleManager.instance.ActivateCombatCamera(currentInteractionHook.transform);

            interaction.StartMethod(this);
            if (interaction.TickIsFinished(this, deltaTime))
            {
                interaction.OnEnd(this);
                currentInteraction = null;
            }
        }

        public override void ActionIsDone()
        {
            currentInteractionInstance.interactionContainer.action.ActionDone(this);
        }
        public override void LoadActionFromInteractionContainer(InteractionInstance instance)
        {
            currentInteractionInstance = instance;
            currentInteractionInstance.interactionContainer.LoadAction(this);
        }

        //We shall see what to do with this, it messes things up now should probably be retaliation
        //Atm used for debug purposes cause not all animations have hit events
        public void HitReceivedCompleted()
        {
            currentInteraction = null;

            if (isTargetPointBlank)
                isTargetPointBlank = false;
        }
        public override void InteractionCompleted()
        {
            currentInteraction = null;
            currentInteractionHook = null;

            if (isTargetPointBlank)
                isTargetPointBlank = false;

            //Maaybe keep interaction UI this for some cool inteeractions tracking
            if (currentInteractionInstance.uiObject != null)
            {
                currentInteractionInstance.uiObject.SetToDestroy();
                if (InteractionButton.instance != null)
                {
                    InteractionButton.instance.OnClick();
                }
            }

            //If animation doesn't have hit event set
            if (!BattleManager.instance.unitReceivedHitDebug)
                BattleManager.instance.CurrentCombatEvent.OnDamageReceived();

            BattleManager.instance.OnMoveFinished();
        }
    }
}