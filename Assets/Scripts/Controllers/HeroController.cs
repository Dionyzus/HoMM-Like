using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class HeroController : GridUnit
    {
        bool failedToLoadPath = false;

        public RenderTexture renderTexture;
        public Image heroImage;

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

        [Header("Public")]
        public Inventory Inventory;
        public ArtifactsPanel ArtifactsPanel;

        [Header("Serialize Field")]
        [SerializeField] StatPanel statPanel;
        [SerializeField] ItemTooltip itemTooltip;
        [SerializeField] Image draggableItem;
        [SerializeField] ItemSaveManager itemSaveManager;

        private BaseItemSlot dragItemSlot;
        private ItemContainer openItemContainer;

        public StatPanel StatPanel { get => statPanel; set => statPanel = value; }
        public ItemTooltip ItemTooltip { get => itemTooltip; set => itemTooltip = value; }
        public Image DraggableItem { get => draggableItem; set => draggableItem = value; }
        public ItemSaveManager ItemSaveManager { get => itemSaveManager; set => itemSaveManager = value; }

        private void OnValidate()
        {
            if (ItemTooltip == null)
                ItemTooltip = FindObjectOfType<ItemTooltip>();
        }
        private void Awake()
        {
            gridIndex = basicHeroStats.gridIndex;
            stepsCount = basicHeroStats.stepsCount;

            movementSpeed = basicHeroStats.movementSpeed;
            rotationSpeed = basicHeroStats.rotationSpeed;

            StatPanel.SetStats(Logistics, Luck, Attack, Defense);
            StatPanel.UpdateStatValues();

            Inventory.OnRightClickEvent += InventoryRightClick;
            ArtifactsPanel.OnRightClickEvent += EquipmentPanelRightClick;

            Inventory.OnPointerEnterEvent += ShowTooltip;
            ArtifactsPanel.OnPointerEnterEvent += ShowTooltip;

            Inventory.OnPointerExitEvent += HideTooltip;
            ArtifactsPanel.OnPointerExitEvent += HideTooltip;

            Inventory.OnBeginDragEvent += BeginDrag;
            ArtifactsPanel.OnBeginDragEvent += BeginDrag;

            Inventory.OnEndDragEvent += EndDrag;
            ArtifactsPanel.OnEndDragEvent += EndDrag;

            Inventory.OnDragEvent += Drag;
            ArtifactsPanel.OnDragEvent += Drag;

            Inventory.OnDropEvent += Drop;
            ArtifactsPanel.OnDropEvent += Drop;
        }

        private void Start()
        {
            if (ItemSaveManager != null)
            {
                ItemSaveManager.LoadInventory(this);
                ItemSaveManager.LoadEquipment(this);
            }

            Inventory.SetHeroImage(this);
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
            if (InteractionSlider.value == 0)
            {
                UiManager.instance.ResetInteractionSlider(this);
            }
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

                if (PathfinderMaster.instance.pathLine.positionCount > 0)
                    PathfinderMaster.instance.pathLine.positionCount -= 1;
                SetInteractionSliderStatus();

                if (index > currentPath.Count - 1)
                {
                    pathIsFinished = true;
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
                if (PathfinderMaster.instance.pathLine.positionCount > 0)
                {
                    PathfinderMaster.instance.pathLine.positionCount = 0;
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
        public override void InteractionCompleted()
        {
            currentInteraction = null;

            Destroy(currentInteractionHook.gameObject);
            currentInteractionHook = null;

            if (currentInteractionInstance.uiObject != null)
            {
                currentInteractionInstance.uiObject.SetToDestroy();
                if (InteractionButton.instance != null)
                {
                    InteractionButton.instance.OnClick();
                }
            }

            WorldManager.instance.DeactivateLookAtActionCamera();
        }

        public override void InitializeMoveToInteractionContainer(Node targetNode)
        {
            InteractionInstance ii = new InteractionInstance
            {
                interactionContainer = moveToLocationContainer,
                gridUnit = this
            };

            interactionInstance = ii;

            UiManager.instance.CreateUiObjectForInteraction(ii);
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
                ItemTooltip.ShowTooltip(itemSlot.Item);
            }
        }

        private void HideTooltip(BaseItemSlot itemSlot)
        {
            if (ItemTooltip.gameObject.activeSelf)
            {
                ItemTooltip.HideTooltip();
            }
        }

        private void BeginDrag(BaseItemSlot itemSlot)
        {
            if (itemSlot.Item != null)
            {
                dragItemSlot = itemSlot;
                DraggableItem.sprite = itemSlot.Item.Icon;
                DraggableItem.transform.position = GameManager.instance.Mouse.position.ReadValue();
                DraggableItem.gameObject.SetActive(true);
            }
        }

        private void Drag(BaseItemSlot itemSlot)
        {
            DraggableItem.transform.position = GameManager.instance.Mouse.position.ReadValue();
        }

        private void EndDrag(BaseItemSlot itemSlot)
        {
            dragItemSlot = null;
            DraggableItem.gameObject.SetActive(false);
        }

        private void Drop(BaseItemSlot dropItemSlot)
        {
            if (dragItemSlot == null) return;

            if (dropItemSlot.CanAddStack(dragItemSlot.Item))
            {
                AddStacks(dropItemSlot);
            }
            else if (dropItemSlot.CanReceiveItem(dragItemSlot.Item) && dragItemSlot.CanReceiveItem(dropItemSlot.Item))
            {
                SwapItems(dropItemSlot);
            }
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
            StatPanel.UpdateStatValues();

            Item draggedItem = dragItemSlot.Item;
            int draggedItemAmount = dragItemSlot.Amount;

            dragItemSlot.Item = dropItemSlot.Item;
            dragItemSlot.Amount = dropItemSlot.Amount;

            dropItemSlot.Item = draggedItem;
            dropItemSlot.Amount = draggedItemAmount;
        }

        public void Equip(EquippableItem item)
        {
            if (Inventory.RemoveItem(item))
            {
                EquippableItem previousItem;
                if (ArtifactsPanel.AddItem(item, out previousItem))
                {
                    if (previousItem != null)
                    {
                        Inventory.AddItem(previousItem);
                        previousItem.Unequip(this);
                        StatPanel.UpdateStatValues();
                    }
                    item.Equip(this);
                    StatPanel.UpdateStatValues();
                }
                else
                {
                    Inventory.AddItem(item);
                }
            }
        }

        public void Unequip(EquippableItem item)
        {
            if (Inventory.CanAddItem(item) && ArtifactsPanel.RemoveItem(item))
            {
                item.Unequip(this);
                StatPanel.UpdateStatValues();
                Inventory.AddItem(item);
            }
        }

        private void TransferToItemContainer(BaseItemSlot itemSlot)
        {
            Item item = itemSlot.Item;
            if (item != null && openItemContainer.CanAddItem(item))
            {
                Inventory.RemoveItem(item);
                openItemContainer.AddItem(item);
            }
        }

        private void TransferToInventory(BaseItemSlot itemSlot)
        {
            Item item = itemSlot.Item;
            if (item != null && Inventory.CanAddItem(item))
            {
                openItemContainer.RemoveItem(item);
                Inventory.AddItem(item);
            }
        }

        public void OpenItemContainer(ItemContainer itemContainer)
        {
            openItemContainer = itemContainer;

            Inventory.OnRightClickEvent -= InventoryRightClick;
            Inventory.OnRightClickEvent += TransferToItemContainer;

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

            Inventory.OnRightClickEvent += InventoryRightClick;
            Inventory.OnRightClickEvent -= TransferToItemContainer;

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