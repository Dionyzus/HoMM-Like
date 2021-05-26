using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class InventoryReference : MonoBehaviour
    {
        [Header("Public")]
        public Inventory Inventory;
        public ArtifactsPanel ArtifactsPanel;

        [Header("Serialize Field")]
        [SerializeField] StatPanel statPanel;
        [SerializeField] ItemTooltip itemTooltip;
        [SerializeField] Image draggableItem;
        [SerializeField] ItemSaveManager itemSaveManager;

        public StatPanel StatPanel { get => statPanel; set => statPanel = value; }
        public ItemTooltip ItemTooltip { get => itemTooltip; set => itemTooltip = value; }
        public Image DraggableItem { get => draggableItem; set => draggableItem = value; }
        public ItemSaveManager ItemSaveManager { get => itemSaveManager; set => itemSaveManager = value; }
    }
}