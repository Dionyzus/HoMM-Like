using System;
using UnityEngine;

namespace HOMM_BM
{
    public class ArtifactsPanel : MonoBehaviour
    {
        public ArtifactSlot[] artifactSlots;
        [SerializeField] Transform artifactSlotsParent;

        public Transform EquipmentSlotsParent { get => artifactSlotsParent; set => artifactSlotsParent = value; }

        public event Action<BaseItemSlot> OnPointerEnterEvent;
        public event Action<BaseItemSlot> OnPointerExitEvent;
        public event Action<BaseItemSlot> OnRightClickEvent;
        public event Action<BaseItemSlot> OnBeginDragEvent;
        public event Action<BaseItemSlot> OnEndDragEvent;
        public event Action<BaseItemSlot> OnDragEvent;
        public event Action<BaseItemSlot> OnDropEvent;

        public void Initialize()
        {
            artifactSlots = EquipmentSlotsParent.GetComponentsInChildren<ArtifactSlot>();

            for (int i = 0; i < artifactSlots.Length; i++)
            {
                artifactSlots[i].OnPointerEnterEvent += slot => OnPointerEnterEvent(slot);
                artifactSlots[i].OnPointerExitEvent += slot => OnPointerExitEvent(slot);
                artifactSlots[i].OnRightClickEvent += slot => OnRightClickEvent(slot);
                artifactSlots[i].OnBeginDragEvent += slot => OnBeginDragEvent(slot);
                artifactSlots[i].OnEndDragEvent += slot => OnEndDragEvent(slot);
                artifactSlots[i].OnDragEvent += slot => OnDragEvent(slot);
                artifactSlots[i].OnDropEvent += slot => OnDropEvent(slot);
            }
        }

        public bool AddItem(EquippableItem item, out EquippableItem previousItem)
        {
            for (int i = 0; i < artifactSlots.Length; i++)
            {
                if (artifactSlots[i].ArtifactType == item.ArtifactType)
                {
                    previousItem = (EquippableItem)artifactSlots[i].Item;
                    artifactSlots[i].Item = item;
                    artifactSlots[i].Amount = 1;
                    return true;
                }
            }
            previousItem = null;
            return false;
        }

        public bool RemoveItem(EquippableItem item)
        {
            for (int i = 0; i < artifactSlots.Length; i++)
            {
                if (artifactSlots[i].Item == item)
                {
                    artifactSlots[i].Item = null;
                    artifactSlots[i].Amount = 0;
                    return true;
                }
            }
            return false;
        }
    }
}