using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace HOMM_BM
{
    public class DialogPrompt : MonoBehaviour
    {
        public event Action OnYesEvent;
        public event Action OnNoEvent;

        public void Show()
        {
            this.gameObject.SetActive(true);
            OnYesEvent = null;
            OnNoEvent = null;
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public void OnYesButtonClick()
        {
            OnYesEvent?.Invoke();

            Hide();
        }

        public void OnNoButtonClick()
        {
            OnNoEvent?.Invoke();

            Hide();
        }
    }
}