using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class HeroInteractionHook : InteractionHook
    {
        [SerializeField]
        ItemAmountDictionary items = new ItemAmountDictionary();
        public ItemAmountDictionary Items { get => items; set => items = value; }
    }
}