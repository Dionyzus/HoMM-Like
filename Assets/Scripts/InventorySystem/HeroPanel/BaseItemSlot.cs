using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace HOMM_BM
{
    public class BaseItemSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] protected Image image;
        [SerializeField] protected TextMeshProUGUI amountText;

        public event Action<BaseItemSlot> OnPointerEnterEvent;
        public event Action<BaseItemSlot> OnPointerExitEvent;
        public event Action<BaseItemSlot> OnRightClickEvent;

        protected bool isPointerOver;

        protected Color normalColor = Color.white;
        protected Color disabledColor = new Color(1, 1, 1, 0);

        protected Item item;
        public Item Item
        {
            get { return item; }
            set
            {
                item = value;
                if (item == null && Amount != 0) Amount = 0;

                if (item == null)
                {
                    if (image != null)
                    {
                        image.sprite = null;
                        image.color = disabledColor;
                    }
                }
                else
                {
                    if (image != null)
                    {
                        image.sprite = item.Icon;
                        image.color = normalColor;
                    }
                }

                if (isPointerOver)
                {
                    OnPointerExit(null);
                    OnPointerEnter(null);
                }
            }
        }

        private int amount;
        public int Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                if (amount < 0) amount = 0;
                if (amount == 0 && Item != null) Item = null;

                if (amountText != null)
                {
                    amountText.enabled = item != null && amount > 1;
                    if (amountText.enabled)
                    {
                        amountText.text = amount.ToString();
                    }
                }
            }
        }

        public virtual bool CanAddStack(Item item, int amount = 1)
        {
            return Item != null && Item.ID == item.ID;
        }

        public virtual bool CanReceiveItem(Item item)
        {
            return false;
        }

        protected virtual void OnValidate()
        {
            if (image == null)
                image = GetComponent<Image>();

            if (amountText == null)
                amountText = GetComponentInChildren<TextMeshProUGUI>();

            Item = item;
            Amount = amount;
        }

        protected virtual void OnDisable()
        {
            if (isPointerOver)
            {
                OnPointerExit(null);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null && eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClickEvent?.Invoke(this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isPointerOver = true;
            OnPointerEnterEvent?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerOver = false;
            OnPointerExitEvent?.Invoke(this);
        }
    }
}