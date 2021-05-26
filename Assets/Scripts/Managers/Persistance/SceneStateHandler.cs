using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HOMM_BM
{
    public class SceneStateHandler : MonoBehaviour
    {
        public static SceneStateHandler instance;
        void Awake()
        {
            instance = this;
        }
        public void SetActiveState(string keyName, GameObject go, bool state)
        {
            PlayerPrefs.SetInt(keyName, state ? 1 : 0);
            go.SetActive(state);
        }
        public void UpdateActiveState(string keyName, GameObject go)
        {
            if (PlayerPrefs.GetInt(keyName) == 1)
            {
                go.SetActive(false);
            }
            else
            {
                go.SetActive(true);
            }
        }
    }
}