using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HOMM_BM
{
    public class ItemTooltip : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI ItemNameText;
        [SerializeField] TextMeshProUGUI ItemTypeText;
        [SerializeField] TextMeshProUGUI ItemDescriptionText;

        public TextMeshProUGUI ItemNameText1 { get => ItemNameText; set => ItemNameText = value; }
        public TextMeshProUGUI ItemTypeText1 { get => ItemTypeText; set => ItemTypeText = value; }
        public TextMeshProUGUI ItemDescriptionText1 { get => ItemDescriptionText; set => ItemDescriptionText = value; }

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void ShowTooltip(Item item)
        {
            ItemNameText1.text = item.ItemName;
            ItemTypeText1.text = item.GetItemType();
            ItemDescriptionText1.text = item.GetDescription();
            gameObject.SetActive(true);
        }

        public void HideTooltip()
        {
            gameObject.SetActive(false);
        }
    }
}