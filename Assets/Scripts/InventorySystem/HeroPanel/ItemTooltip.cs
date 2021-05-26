using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HOMM_BM
{
    public class ItemTooltip : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI itemNameText;
        [SerializeField] TextMeshProUGUI itemTypeText;
        [SerializeField] TextMeshProUGUI itemDescriptionText;

        public TextMeshProUGUI ItemNameText { get => itemNameText; set => itemNameText = value; }
        public TextMeshProUGUI ItemTypeText { get => itemTypeText; set => itemTypeText = value; }
        public TextMeshProUGUI ItemDescriptionText { get => itemDescriptionText; set => itemDescriptionText = value; }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void ShowTooltip(Item item)
        {
            ItemNameText.text = item.ItemName;
            ItemTypeText.text = item.GetItemType();
            ItemDescriptionText.text = item.GetDescription();
            gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
    }
}