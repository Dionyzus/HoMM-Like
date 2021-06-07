using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System;

namespace HOMM_BM
{
    public class HeroController : GridUnit
    {
        bool failedToLoadPath = false;

        int actionPoints;

        public RenderTexture renderTexture;
        public Image heroImage;
        public string heroName = "";

        public GameObject heroModel;

        public BasicHeroStats basicHeroStats;

        private Slider interactionSlider;
        public Slider InteractionSlider { get => interactionSlider; set => interactionSlider = value; }

        private bool isInteractionPointBlank;
        public bool IsInteractionPointBlank { get => isInteractionPointBlank; set => isInteractionPointBlank = value; }

        public InteractionContainer moveToLocationContainer;

        [Header("Stats")]
        public HeroStat Logistics;
        public HeroStat Luck;
        public HeroStat Attack;
        public HeroStat Defense;

        Inventory inventory;
        ArtifactsPanel artifactsPanel;

        StatPanel statPanel;
        ItemTooltip itemTooltip;
        Image draggableItem;
        ItemSaveManager itemSaveManager;

        private BaseItemSlot dragItemSlot;
        private ItemContainer openItemContainer;

        DialogPrompt reallyEnterTheBattlePrompt;
        SplitArmyPrompt splitArmyPrompt;

        public StatPanel StatPanel { get => statPanel; set => statPanel = value; }
        public ItemTooltip ItemTooltip { get => itemTooltip; set => itemTooltip = value; }
        public Image DraggableItem { get => draggableItem; set => draggableItem = value; }
        public ItemSaveManager ItemSaveManager { get => itemSaveManager; set => itemSaveManager = value; }
        public InventoryReference InventoryReference { get => inventoryReference; set => inventoryReference = value; }
        public Inventory Inventory { get => inventory; set => inventory = value; }
        public ArtifactsPanel ArtifactsPanel { get => artifactsPanel; set => artifactsPanel = value; }
        public DialogPrompt ReallyEnterTheBattlePrompt { get => reallyEnterTheBattlePrompt; set => reallyEnterTheBattlePrompt = value; }
        public SplitArmyPrompt SplitArmyPrompt { get => splitArmyPrompt; set => splitArmyPrompt = value; }
        public int ActionPoints { get => actionPoints; set => actionPoints = value; }
        public AudioListener AudioListener { get => audioListener; set => audioListener = value; }

        [SerializeField]
        private InventoryReference inventoryReference;

        [SerializeField]
        AudioManager stepSound = default;

        AudioListener audioListener;
        public void InitializeInventory(InventoryReference reference)
        {
            inventoryReference = reference;

            itemSaveManager = inventoryReference.ItemSaveManager;
            inventory = inventoryReference.Inventory;
            statPanel = inventoryReference.StatPanel;
            artifactsPanel = inventoryReference.ArtifactsPanel;
            itemTooltip = inventoryReference.ItemTooltip;
            draggableItem = inventoryReference.DraggableItem;

            statPanel.Initialize();
            statPanel.SetStats(Logistics, Luck, Attack, Defense);
            statPanel.UpdateStatValues();

            artifactsPanel.Initialize();

            inventory.SetHeroImage(heroImage);
            inventory.SetHeroName(heroName);

            inventory.OnRightClickEvent += InventoryRightClick;
            artifactsPanel.OnRightClickEvent += EquipmentPanelRightClick;

            inventory.OnPointerEnterEvent += ShowTooltip;
            artifactsPanel.OnPointerEnterEvent += ShowTooltip;

            inventory.OnPointerExitEvent += HideTooltip;
            artifactsPanel.OnPointerExitEvent += HideTooltip;

            inventory.OnBeginDragEvent += BeginDrag;
            artifactsPanel.OnBeginDragEvent += BeginDrag;

            inventory.OnEndDragEvent += EndDrag;
            artifactsPanel.OnEndDragEvent += EndDrag;

            inventory.OnDragEvent += Drag;
            artifactsPanel.OnDragEvent += Drag;

            inventory.OnDropEvent += Drop;
            artifactsPanel.OnDropEvent += Drop;

            if (itemSaveManager != null)
            {
                itemSaveManager.LoadInventory(this);
                itemSaveManager.LoadEquipment(this);
            }
        }

        private void Awake()
        {
            gridIndex = basicHeroStats.gridIndex;
            stepsCount = basicHeroStats.stepsCount;

            movementSpeed = basicHeroStats.movementSpeed;
            rotationSpeed = basicHeroStats.rotationSpeed;
        }

        private void OnDestroy()
        {
            if (ItemSaveManager != null)
            {
                ItemSaveManager.SaveEquipment(this);
                ItemSaveManager.SaveInventory(this);
            }
        }

        private void Update()
        {

            float deltaTime = Time.deltaTime;

            //Check this out...
            //animator.applyRootMotion = IsInteracting;
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            animator.SetBool("isWalking", isWalking);

            if (currentPath.Count > 0)
            {
                HandleMovement(deltaTime);
            }
            else
            {
                isWalking = false;
            }

            if (isInteractionInitialized)
            {
                if (interactionInstance != null)
                {
                    LoadActionFromInteractionContainer(interactionInstance);
                    interactionInstance = null;
                }
            }

            if (currentInteraction != null)
            {
                HandleInteraction(currentInteraction, deltaTime);
            }
        }

        public void SetInteractionSliderStatus()
        {
            InteractionSlider.value -= 1;
        }
        public void SetEnemyTurnSliderStatus()
        {
            UiManager.instance.currentEnemyTurnSlider.value += 1;
        }

        void HandleMovement(float deltaTime)
        {
            isWalking = false;

            if (time > 0)
            {
                time -= deltaTime;
                return;
            }

            if (!isPathInitialized)
            {
                originPosition = CurrentNode.worldPosition;
                targetPosition = currentPath[index].worldPosition;
                float distance = Vector3.Distance(originPosition, targetPosition);
                actualMovementSpeed = distance / movementSpeed;

                targetRotation = Quaternion.LookRotation((targetPosition - originPosition).normalized);
                moveTime = 0;

                isPathInitialized = true;

                if (gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                    CursorManager.instance.SetToInteractionInitialized();
            }

            bool pathIsFinished = false;

            isWalking = true;

            moveTime += deltaTime / actualMovementSpeed;
            rotationTime += deltaTime / rotationSpeed;

            if (rotationTime > 1)
            {
                rotationTime = 1;
            }

            if (moveTime > 1)
            {
                moveTime = 1;
                isPathInitialized = false;
                index++;

                if (PathfinderMaster.instance.pathLineInRange.positionCount > 0)
                    PathfinderMaster.instance.pathLineInRange.positionCount -= 1;

                if (this.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                    SetInteractionSliderStatus();
                else
                    SetEnemyTurnSliderStatus();

                if (index > actionPoints - 1)
                {
                    pathIsFinished = true;
                    if (gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                        CursorManager.instance.SetToDefault();
                }
                else if (index > currentPath.Count - 1)
                {
                    actionPoints -= currentPath.Count;
                    pathIsFinished = true;
                    if (gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
                        CursorManager.instance.SetToDefault();
                }
            }

            Vector3 tp = Vector3.Lerp(originPosition, targetPosition, moveTime);
            transform.position = tp;

            Vector3 direction = (targetPosition - originPosition).normalized;
            direction.y = 0;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime / rotationSpeed);

            if (pathIsFinished)
            {
                currentPath.Clear();
                onPathReachCallback?.Invoke();
            }
        }
        public void PlayFootstep()
        {
            stepSound.Play();
        }
        public override void CreateInteractionContainer(InteractionContainer container)
        {
            InteractionInstance ii = new InteractionInstance
            {
                interactionContainer = container,
                gridUnit = this
            };
            interactionInstance = ii;

            UiManager.instance.CreateUiObjectForInteraction(ii, container.moveDisplay);
        }
        public override void StoreInteractionHook(InteractionHook interactionHook)
        {
            currentInteractionHook = interactionHook;
            PathfindToInteractionHook();
        }
        void PathfindToInteractionHook()
        {
            MoveToRequestedLocation(LoadInteractionFromInteractionHook);
        }
        public void MoveToRequestedLocation(OnPathReachCallback callback)
        {
            if (isInteractionPointBlank)
            {
                LoadInteractionFromInteractionHook();
                if (PathfinderMaster.instance.pathLineInRange.positionCount > 0)
                {
                    PathfinderMaster.instance.pathLineInRange.positionCount = 0;
                }
                isInteractionPointBlank = false;
                return;
            }

            PathfinderMaster.instance.RequestPathfinder(LoadPath, callback);
        }
        public void PreviewPathToNode(Node target, InteractionHook interactionHook = null)
        {
            if (interactionHook != null)
            {
                PathfinderMaster.instance.RequestPathAndPreview(CurrentNode,
                    GridManager.instance.GetNode(interactionHook.interactionPoint.position, gridIndex), this);
            }
            else
            {
                if (!target.IsWalkable())
                {
                    Debug.Log("Target node is not walkable");
                }

                PathfinderMaster.instance.RequestPathAndPreview(CurrentNode, target, this);
            }
        }
        public void LoadPath(List<Node> path, OnPathReachCallback callback)
        {
            if (path == null || path.Count == 0)
            {
                Debug.Log("Failed to load path, path is null.");

                failedToLoadPath = true;
                MovingToLocationCompleted();

                return;
            }
            else
            {
                currentPath = path;
                index = 0;
                isPathInitialized = false;
                onPathReachCallback = callback;
            }
        }
        void LoadInteractionFromInteractionHook()
        {
            currentInteractionHook.LoadInteraction(this);
            isInteractionInitialized = false;
        }
        public override void LoadInteraction(Interaction targetInteraction)
        {
            currentInteraction = targetInteraction;
        }
        protected override void HandleInteraction(Interaction interaction, float deltaTime)
        {
            interaction.StartMethod(this);

            Image image = currentInteractionInstance.uiObject.GetComponentInChildren<Image>();
            image.sprite = currentInteractionInstance.interactionContainer.interactDisplay;

            if (interaction.TickIsFinished(this, deltaTime))
            {
                interaction.OnEnd(this);
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
        public override void InteractionCompleted()
        {
            if (currentInteraction != null && currentInteraction.GetType() != typeof(SceneTriggerInteraction) && currentInteraction.GetType() != typeof(InteractWithHeroInteraction))
            {
                InteractionHook interactionHook = SceneStateHandler.instance.InteractionHooks
                    .Find(hook => hook.GetInstanceID() == currentInteractionHook.GetInstanceID());

                SceneStateHandler.instance.UpdateActiveState(interactionHook.transform.name);
                SceneStateHandler.instance.InteractionHooks.Remove(interactionHook);

                if (currentInteractionHook.interactionAnimation.Length == 0)
                    Destroy(currentInteractionHook.gameObject);
                ClearInteractionData();
            }

            WorldManager.instance.DeactivateLookAtActionCamera();
        }

        public override void ClearInteractionData()
        {
            currentInteraction = null;
            currentInteractionHook = null;

            if (currentInteractionInstance.uiObject != null)
            {
                currentInteractionInstance.uiObject.SetToDestroy();
                if (InteractionButton.instance != null)
                {
                    InteractionButton.instance.OnClick();
                }
            }
        }

        public override void InitializeMoveToInteractionContainer(Node targetNode)
        {
            InteractionInstance ii = new InteractionInstance
            {
                interactionContainer = moveToLocationContainer,
                gridUnit = this
            };

            interactionInstance = ii;

            UiManager.instance.CreateUiObjectForInteraction(ii, moveToLocationContainer.moveDisplay);
        }
        public override void MoveToLocation()
        {
            MoveToRequestedLocation(
                    MovingToLocationCompleted);
        }
        public override void MovingToLocationCompleted()
        {
            if (isInteractionInitialized)
                isInteractionInitialized = false;

            if (failedToLoadPath)
            {
                Debug.Log("Do some animation, maybe interaction button a bit diff");
                failedToLoadPath = false;
            }

            if (currentInteractionInstance.uiObject != null)
            {
                currentInteractionInstance.uiObject.SetToDestroy();
                if (InteractionButton.instance != null)
                {
                    InteractionButton.instance.OnClick();
                }
            }

            PathfinderMaster.instance.ClearCreatedNodes();

            WorldManager.instance.DeactivateLookAtActionCamera();
            //Temporary, since AI hero is only able to move
            if (this.gameObject.layer == GridManager.ENEMY_UNITS_LAYER)
                WorldManager.instance.OnMoveFinished();
        }

        //Inventory
        private void InventoryRightClick(BaseItemSlot itemSlot)
        {
            if (itemSlot.Item is EquippableItem)
            {
                Equip((EquippableItem)itemSlot.Item);
            }
        }

        private void EquipmentPanelRightClick(BaseItemSlot itemSlot)
        {
            if (itemSlot.Item is EquippableItem)
            {
                Unequip((EquippableItem)itemSlot.Item);
            }
        }

        private void ShowTooltip(BaseItemSlot itemSlot)
        {
            if (itemSlot.Item != null)
            {
                itemTooltip.ShowTooltip(itemSlot.Item);
            }
        }

        private void HideTooltip(BaseItemSlot itemSlot)
        {
            if (itemTooltip.gameObject.activeSelf)
            {
                itemTooltip.HideTooltip();
            }
        }

        private void BeginDrag(BaseItemSlot itemSlot)
        {
            if (itemSlot.Item != null)
            {
                dragItemSlot = itemSlot;
                draggableItem.sprite = itemSlot.Item.Icon;
                draggableItem.transform.position = GameManager.instance.Mouse.position.ReadValue();
                draggableItem.gameObject.SetActive(true);
            }
        }

        private void Drag(BaseItemSlot itemSlot)
        {
            draggableItem.transform.position = GameManager.instance.Mouse.position.ReadValue();
        }

        private void EndDrag(BaseItemSlot itemSlot)
        {
            dragItemSlot = null;
            draggableItem.gameObject.SetActive(false);
        }

        private void Drop(BaseItemSlot dropItemSlot)
        {
            if (dragItemSlot == null) return;

            if (dropItemSlot.CanReceiveItem(dragItemSlot.Item) && dragItemSlot.CanReceiveItem(dropItemSlot.Item) && GameManager.instance.Keyboard.shiftKey.isPressed)
            {
                if (!dragItemSlot.Item.GetType().Equals(typeof(UnitItem)))
                    return;
                if (dragItemSlot.Amount < 2)
                    return;

                BaseItemSlot itemSlotReference = dragItemSlot;

                InitializeSplitArmyWindow(itemSlotReference, splitArmyPrompt);

                splitArmyPrompt.Show();
                splitArmyPrompt.OnYesEvent += () => SplitArmy(dropItemSlot, itemSlotReference, splitArmyPrompt);
            }
            else if (dropItemSlot.CanAddStack(dragItemSlot.Item))
            {
                AddStacks(dropItemSlot);
            }
            else if (dropItemSlot.CanReceiveItem(dragItemSlot.Item) && dragItemSlot.CanReceiveItem(dropItemSlot.Item))
            {
                SwapItems(dropItemSlot);
            }
        }
        public void InitializeSplitArmyWindow(BaseItemSlot itemSlot, SplitArmyPrompt dialogPrompt)
        {
            dialogPrompt.Slider.maxValue = itemSlot.Amount;
            dialogPrompt.MaxValue = itemSlot.Amount;
            dialogPrompt.Slider.minValue = 1;

            int leftValue = Mathf.FloorToInt(itemSlot.Amount / 2);
            int rightValue = itemSlot.Amount - leftValue;

            dialogPrompt.Slider.value = leftValue;

            dialogPrompt.FromImage.sprite = itemSlot.Item.Icon;
            dialogPrompt.ToImage.sprite = itemSlot.Item.Icon;

            dialogPrompt.FromText.text = leftValue.ToString();
            dialogPrompt.ToText.text = rightValue.ToString();
        }
        public void SplitArmy(BaseItemSlot dropItemSlot, BaseItemSlot itemSlot, SplitArmyPrompt dialogPrompt)
        {
            itemSlot.Amount = int.Parse(dialogPrompt.FromText.text);

            int newItemAmount = int.Parse(dialogPrompt.ToText.text);

            UnitItem originalItemInSlot = (UnitItem)itemSlot.Item;
            UnitItem newUnitItem = CreateCopyFromItem(originalItemInSlot);

            if (newUnitItem != null)
                inventory.AddItemsToItemSlot(dropItemSlot, newUnitItem, newItemAmount);
        }
        UnitItem CreateCopyFromItem(UnitItem originItem)
        {
            UnitItem newUnitItem = ScriptableObject.CreateInstance<UnitItem>();

            newUnitItem.ID = originItem.ID;
            newUnitItem.ItemName = originItem.ItemName;
            newUnitItem.MaximumStacks = originItem.MaximumStacks;
            newUnitItem.Icon = originItem.Icon;
            newUnitItem.UnitAttackType = originItem.UnitAttackType;
            newUnitItem.UnitType = originItem.UnitType;
            newUnitItem.UnitStats = originItem.UnitStats;

            return newUnitItem;
        }
        private void AddStacks(BaseItemSlot dropItemSlot)
        {
            int numAddableStacks = dropItemSlot.Item.MaximumStacks - dropItemSlot.Amount;
            int stacksToAdd = Mathf.Min(numAddableStacks, dragItemSlot.Amount);

            dropItemSlot.Amount += stacksToAdd;
            dragItemSlot.Amount -= stacksToAdd;
        }

        private void SwapItems(BaseItemSlot dropItemSlot)
        {
            EquippableItem dragEquipItem = dragItemSlot.Item as EquippableItem;
            EquippableItem dropEquipItem = dropItemSlot.Item as EquippableItem;

            if (dropItemSlot is ArtifactSlot)
            {
                if (dragEquipItem != null) dragEquipItem.Equip(this);
                if (dropEquipItem != null) dropEquipItem.Unequip(this);
            }
            if (dragItemSlot is ArtifactSlot)
            {
                if (dragEquipItem != null) dragEquipItem.Unequip(this);
                if (dropEquipItem != null) dropEquipItem.Equip(this);
            }
            statPanel.UpdateStatValues();

            Item draggedItem = dragItemSlot.Item;
            int draggedItemAmount = dragItemSlot.Amount;

            dragItemSlot.Item = dropItemSlot.Item;
            dragItemSlot.Amount = dropItemSlot.Amount;

            dropItemSlot.Item = draggedItem;
            dropItemSlot.Amount = draggedItemAmount;
        }

        public void Equip(EquippableItem item)
        {
            if (inventory.RemoveItem(item))
            {
                EquippableItem previousItem;
                if (artifactsPanel.AddItem(item, out previousItem))
                {
                    if (previousItem != null)
                    {
                        inventory.AddItem(previousItem);
                        previousItem.Unequip(this);
                        statPanel.UpdateStatValues();
                    }
                    item.Equip(this);
                    statPanel.UpdateStatValues();
                }
                else
                {
                    inventory.AddItem(item);
                }
            }
        }

        public void Unequip(EquippableItem item)
        {
            if (inventory.CanAddItem(item) && artifactsPanel.RemoveItem(item))
            {
                item.Unequip(this);
                statPanel.UpdateStatValues();
                inventory.AddItem(item);
            }
        }

        private void TransferToItemContainer(BaseItemSlot itemSlot)
        {
            Item item = itemSlot.Item;
            if (item != null && openItemContainer.CanAddItem(item))
            {
                inventory.RemoveItem(item);
                openItemContainer.AddItem(item);
            }
        }

        private void TransferToInventory(BaseItemSlot itemSlot)
        {
            Item item = itemSlot.Item;
            if (item != null && Inventory.CanAddItem(item))
            {
                openItemContainer.RemoveItem(item);
                inventory.AddItem(item);
            }
        }

        public void OpenItemContainer(ItemContainer itemContainer)
        {
            openItemContainer = itemContainer;

            inventory.OnRightClickEvent -= InventoryRightClick;
            inventory.OnRightClickEvent += TransferToItemContainer;

            itemContainer.OnRightClickEvent += TransferToInventory;

            itemContainer.OnPointerEnterEvent += ShowTooltip;
            itemContainer.OnPointerExitEvent += HideTooltip;
            itemContainer.OnBeginDragEvent += BeginDrag;
            itemContainer.OnEndDragEvent += EndDrag;
            itemContainer.OnDragEvent += Drag;
            itemContainer.OnDropEvent += Drop;
        }

        public void CloseItemContainer(ItemContainer itemContainer)
        {
            openItemContainer = null;

            inventory.OnRightClickEvent += InventoryRightClick;
            inventory.OnRightClickEvent -= TransferToItemContainer;

            itemContainer.OnRightClickEvent -= TransferToInventory;

            itemContainer.OnPointerEnterEvent -= ShowTooltip;
            itemContainer.OnPointerExitEvent -= HideTooltip;
            itemContainer.OnBeginDragEvent -= BeginDrag;
            itemContainer.OnEndDragEvent -= EndDrag;
            itemContainer.OnDragEvent -= Drag;
            itemContainer.OnDropEvent -= Drop;
        }

        public void UpdateStatValues()
        {
            StatPanel.UpdateStatValues();
        }
    }
}