using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace HOMM_BM
{
    public class MainMenu : MonoBehaviour
    {
        bool reverseAnimation = true;

        [SerializeField] GameObject menuParent = default;

        [SerializeField] Button startGameButton = default;
        [SerializeField] Button quitGameButton = default;
        [SerializeField] Button clickAnywhereButton = default;

        [SerializeField] Animation slideMenuAnimation = default;
        [SerializeField] AudioManager backgroundMusic = default;
        [SerializeField] AudioManager buttonClickSound = default;

        public string levelName;

        private void Start()
        {
            clickAnywhereButton.onClick.AddListener(SlideMenu);

            startGameButton.onClick.AddListener(() =>
            {
                StartGame(levelName);
            });

            quitGameButton.onClick.AddListener(QuitGame);
        }

        private void Update()
        {
            if (clickAnywhereButton && clickAnywhereButton.transform.localScale.y >= 0.8f && reverseAnimation == true)
            {
                if (clickAnywhereButton.transform.localScale.y <= 0.81f)
                {
                    reverseAnimation = false;
                }
                clickAnywhereButton.transform.localScale -= new Vector3(0.0005f, 0.0005f, 0.0005f);
            }
            else if (clickAnywhereButton && reverseAnimation == false)
            {
                if (clickAnywhereButton.transform.localScale.y >= 0.9f)
                {
                    reverseAnimation = true;
                }
                clickAnywhereButton.transform.localScale += new Vector3(0.0005f, 0.0005f, 0.0005f);
            }
        }
        public void SlideMenu()
        {
            slideMenuAnimation.Play();

            clickAnywhereButton.gameObject.SetActive(false);

            backgroundMusic.Play();
        }

        public void StartGame(string name)
        {
            buttonClickSound.Play();
            backgroundMusic.Stop();

            Cursor.lockState = CursorLockMode.Confined;

            GameReferencesManager.instance.LoadTargetScene(name);
            menuParent.SetActive(false);
        }

        //Better idea is to create own button which would
        //have audio field
        public void PlayClickSound()
        {
            buttonClickSound.Play();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}