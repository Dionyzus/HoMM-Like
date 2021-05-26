using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace HOMM_BM
{
    public class MainMenu : MonoBehaviour
    {
        public void StartGame()
        {
            GameReferencesManager.instance.LoadTargetScene("WorldMap");
        }
    }
}