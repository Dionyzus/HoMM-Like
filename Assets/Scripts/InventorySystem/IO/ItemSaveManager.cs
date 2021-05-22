using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class ItemSaveManager : MonoBehaviour
    {
        [SerializeField] ItemDatabase itemDatabase;

        private const string InventoryFileName = "Inventory";
        private const string EquipmentFileName = "Equipment";

        public ItemDatabase ItemDatabase { get => itemDatabase; set => itemDatabase = value; }

        public void LoadInventory(HeroController hero)
        {
            ItemContainerSaveData savedSlots = ItemSaveIO.LoadItems(InventoryFileName);
            if (savedSlots == null) return;
            hero.Inventory.Clear();

            for (int i = 0; i < savedSlots.SavedSlots.Length; i++)
            {
                ItemSlot itemSlot = hero.Inventory.ItemSlots[i];
                ItemSlotSaveData savedSlot = savedSlots.SavedSlots[i];

                if (savedSlot == null)
                {
                    itemSlot.Item = null;
                    itemSlot.Amount = 0;
                }
                else
                {
                    itemSlot.Item = ItemDatabase.GetItemCopy(savedSlot.ItemID);
                    itemSlot.Amount = savedSlot.Amount;
                }
            }
        }

        public void LoadEquipment(HeroController hero)
        {
            ItemContainerSaveData savedSlots = ItemSaveIO.LoadItems(EquipmentFileName);
            if (savedSlots == null) return;

            foreach (ItemSlotSaveData savedSlot in savedSlots.SavedSlots)
            {
                if (savedSlot == null)
                {
                    continue;
                }

                Item item = ItemDatabase.GetItemCopy(savedSlot.ItemID);
                hero.Inventory.AddItem(item);
                hero.Equip((EquippableItem)item);
            }
        }

        public void SaveInventory(HeroController hero)
        {
            SaveItems(hero.Inventory.ItemSlots, InventoryFileName);
        }

        public void SaveEquipment(HeroController hero)
        {
            SaveItems(hero.ArtifactsPanel.artifactSlots, EquipmentFileName);
        }

        private void SaveItems(IList<ItemSlot> itemSlots, string fileName)
        {
            var saveData = new ItemContainerSaveData(itemSlots.Count);

            for (int i = 0; i < saveData.SavedSlots.Length; i++)
            {
                ItemSlot itemSlot = itemSlots[i];

                if (itemSlot.Item == null)
                {
                    saveData.SavedSlots[i] = null;
                }
                else
                {
                    saveData.SavedSlots[i] = new ItemSlotSaveData(itemSlot.Item.ID, itemSlot.Amount);
                }
            }

            ItemSaveIO.SaveItems(saveData, fileName);
        }
    }
}