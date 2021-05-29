using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HOMM_BM
{
    public class SplitArmyPrompt : DialogPrompt
    {
        [SerializeField]
        Slider slider;
        [SerializeField]
        Image fromImage;
        [SerializeField]
        Image toImage;

        [SerializeField]
        Text fromText;
        [SerializeField]
        Text toText;

        int maxValue;

        public Slider Slider { get => slider; set => slider = value; }
        public Text FromText { get => fromText; set => fromText = value; }
        public Text ToText { get => toText; set => toText = value; }
        public Image FromImage { get => fromImage; set => fromImage = value; }
        public Image ToImage { get => toImage; set => toImage = value; }
        public int MaxValue { get => maxValue; set => maxValue = value; }

        public void UpdateSliderValues(float value)
        {
            if ((int)value == maxValue)
                value -= 1;

            int rightValue = (int)value;
            int leftValue = maxValue - rightValue;
            toText.text = rightValue.ToString();
            fromText.text = leftValue.ToString();
        }
    }
}