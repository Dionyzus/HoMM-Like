using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HOMM_BM
{
    public class ObtainedItemDisplay : MonoBehaviour
    {
        [SerializeField]
        Image itemIcon;
        [SerializeField]
        TextMeshProUGUI amountText;

        public Image ItemIcon { get => itemIcon; set => itemIcon = value; }
        public TextMeshProUGUI AmountText { get => amountText; set => amountText = value; }
    }
}