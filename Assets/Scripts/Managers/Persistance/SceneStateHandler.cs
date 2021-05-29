using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HOMM_BM
{
    public class SceneStateHandler : MonoBehaviour
    {
        public static SceneStateHandler instance;

        [SerializeField]
        List<InteractionHook> interactionHooks = new List<InteractionHook>();

        public List<InteractionHook> InteractionHooks { get => interactionHooks; set => interactionHooks = value; }

        void Awake()
        {
            instance = this;
        }
        public void SetActiveState(string keyName, bool state)
        {
            PlayerPrefs.SetInt(keyName, state ? 1 : 0);
        }
        public bool GetActiveState(string keyName)
        {
            if (PlayerPrefs.GetInt(keyName) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void UpdateActiveState(string keyName)
        {
            if (PlayerPrefs.GetInt(keyName) == 1)
            {
                PlayerPrefs.SetInt(keyName, 0);
            }
            else
            {
                PlayerPrefs.SetInt(keyName, 1);
            }
        }
    }
}