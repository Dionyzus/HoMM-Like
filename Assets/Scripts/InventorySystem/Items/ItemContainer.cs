using System;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public abstract class ItemContainer : MonoBehaviour, IItemContainer
    {
        public List<ItemSlot> ItemSlots;

        public event Action<BaseItemSlot> OnPointerEnterEvent;
        public event Action<BaseItemSlot> OnPointerExitEvent;
        public event Action<BaseItemSlot> OnRightClickEvent;
        public event Action<BaseItemSlot> OnBeginDragEvent;
        public event Action<BaseItemSlot> OnEndDragEvent;
        public event Action<BaseItemSlot> OnDragEvent;
        public event Action<BaseItemSlot> OnDropEvent;

        protected virtual void OnValidate()
        {
            GetComponentsInChildren(includeInactive: true, result: ItemSlots);
        }

        protected virtual void Awake()
        {
            for (int i = 0; i < ItemSlots.Count; i++)
            {
                ItemSlots[i].OnPointerEnterEvent += slot => EventHelper(slot, OnPointerEnterEvent);
                ItemSlots[i].OnPointerExitEvent += slot => EventHelper(slot, OnPointerExitEvent);
                ItemSlots[i].OnRightClickEvent += slot => EventHelper(slot, OnRightClickEvent);
                ItemSlots[i].OnBeginDragEvent += slot => EventHelper(slot, OnBeginDragEvent);
                ItemSlots[i].OnEndDragEvent += slot => EventHelper(slot, OnEndDragEvent);
                ItemSlots[i].OnDragEvent += slot => EventHelper(slot, OnDragEvent);
                ItemSlots[i].OnDropEvent += slot => EventHelper(slot, OnDropEvent);
            }
        }

        private void EventHelper(BaseItemSlot itemSlot, Action<BaseItemSlot> action)
        {
            action?.Invoke(itemSlot);
        }

        public virtual bool CanAddItem(Item item, int amount = 1)
        {
            int freeSpaces = 0;

            foreach (ItemSlot itemSlot in ItemSlots)
            {
                if (itemSlot.Item == null || itemSlot.Item.ID == item.ID)
                {
                    freeSpaces += item.MaximumStacks - itemSlot.Amount;
                }
            }
            return freeSpaces >= amount;
        }

        public virtual bool AddItem(Item item)
        {
            for (int i = 0; i < ItemSlots.Count; i++)
            {
                if (ItemSlots[i].CanAddStack(item))
                {
                    ItemSlots[i].Item = item;
                    ItemSlots[i].Amount++;
                    return true;
                }
            }

            for (int i = 0; i < ItemSlots.Count; i++)
            {
                if (ItemSlots[i].Item == null)
                {
                    ItemSlots[i].Item = item;
                    ItemSlots[i].Amount++;
                    return true;
                }
            }
            return false;
        }

        public virtual bool AddItems(Item item, int amount)
        {
            for (int i = 0; i < ItemSlots.Count; i++)
            {
                if (ItemSlots[i].CanAddStack(item))
                {
                    ItemSlots[i].Item = item;
                    ItemSlots[i].Amount += amount;
                    return true;
                }
            }

            for (int i = 0; i < ItemSlots.Count; i++)
            {
                if (ItemSlots[i].Item == null)
                {
                    ItemSlots[i].Item = item;
                    ItemSlots[i].Amount += amount;
                    return true;
                }
            }
            return false;
        }
        public virtual bool RemoveItem(Item item)
        {
            for (int i = 0; i < ItemSlots.Count; i++)
            {
                if (ItemSlots[i].Item == item)
                {
                    ItemSlots[i].Amount--;
                    return true;
                }
            }
            return false;
        }

        public virtual Item RemoveItem(string itemID)
        {
            for (int i = 0; i < ItemSlots.Count; i++)
            {
                Item item = ItemSlots[i].Item;
                if (item != null && item.ID == itemID)
                {
                    ItemSlots[i].Amount--;
                    return item;
                }
            }
            return null;
        }

        public virtual int ItemCount(string itemID)
        {
            int number = 0;

            for (int i = 0; i < ItemSlots.Count; i++)
            {
                Item item = ItemSlots[i].Item;
                if (item != null && item.ID == itemID)
                {
                    number += ItemSlots[i].Amount;
                }
            }
            return number;
        }

        public void Clear()
        {
            for (int i = 0; i < ItemSlots.Count; i++)
            {
                if (ItemSlots[i].Item != null && Application.isPlaying)
                {
                    ItemSlots[i].Item.Destroy();
                }
                ItemSlots[i].Item = null;
                ItemSlots[i].Amount = 0;
            }
        }
    }

}