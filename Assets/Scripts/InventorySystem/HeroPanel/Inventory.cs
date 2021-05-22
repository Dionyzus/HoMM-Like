using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class Inventory : ItemContainer
    {
        [SerializeField] protected Item[] startingItems;
        [SerializeField] protected Transform itemsParent;
        [SerializeField]
        Image heroImage;
        public Image HeroImage { get => heroImage; set => heroImage = value; }

        protected override void OnValidate()
        {
            if (itemsParent != null)
                itemsParent.GetComponentsInChildren(includeInactive: true, result: ItemSlots);

            if (!Application.isPlaying)
            {
                SetStartingItems();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            SetStartingItems();
        }

        private void SetStartingItems()
        {
            Clear();
            foreach (Item item in startingItems)
            {
                if (item != null && !CheckIfItemIsLoaded("Equipment", item.ID))
                    AddItem(item.GetCopy());
            }
        }
        bool CheckIfItemIsLoaded(string file, string itemId)
        {
            ItemContainerSaveData savedSlots = ItemSaveIO.LoadItems(file);
            if (savedSlots == null) return false;

            foreach (ItemSlotSaveData savedSlot in savedSlots.SavedSlots)
            {
                if (savedSlot == null)
                {
                    continue;
                }
                if (savedSlot.ItemID == itemId)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetHeroImage(HeroController hero)
        {
            heroImage.sprite = hero.heroImage.sprite;
        }
    }
}