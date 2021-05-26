using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace HOMM_BM
{
    public class GameReferencesManager : MonoBehaviour
    {
        public static GameReferencesManager instance;

        HeroController heroController;
        SimpleHero simpleHero;

        GameObject sceneTriggerGo;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public void LoadTargetScene(string sceneName)
        {
            StartCoroutine(LoadScene(sceneName));
        }

        IEnumerator LoadScene(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            while (!operation.isDone)
            {
                yield return null;
            }

            GameManager.instance.StateInitialized = false;

            if (GameManager.instance.CurrentGameState == GameState.WORLD && heroController != null)
            {
                if (simpleHero != null)
                {
                    Destroy(simpleHero.gameObject);
                    simpleHero = null;
                }
                if (sceneTriggerGo != null)
                    SceneStateHandler.instance.UpdateActiveState("SceneTrigger", sceneTriggerGo);

                heroController.gameObject.SetActive(true);
            }

            if (GameManager.instance.CurrentGameState == GameState.WORLD && heroController == null)
            {
                if (sceneTriggerGo == null)
                {
                    sceneTriggerGo = Instantiate(ResourcesManager.Instance.sceneTrigger);
                    sceneTriggerGo.transform.position = ResourcesManager.Instance.sceneTriggerSpawnPosition.position;
                }

                SceneStateHandler.instance.SetActiveState("SceneTrigger", sceneTriggerGo, true);

                InitializeHeroController();
            }
            if (GameManager.instance.CurrentGameState == GameState.BATTLE && simpleHero == null)
            {
                InitializeSimpleHero();
            }
        }
        void InitializeHeroController()
        {
            if (simpleHero != null)
            {
                Destroy(simpleHero.gameObject);
                simpleHero = null;
            }

            GameObject heroControllerGo = Instantiate(ResourcesManager.Instance.heroController);
            heroController = heroControllerGo.GetComponentInChildren<HeroController>();

            heroController.transform.position = ResourcesManager.Instance.heroControllerSpawnPosition.position;
            heroController.transform.rotation = ResourcesManager.Instance.heroControllerSpawnPosition.rotation;

            heroController.transform.SetParent(this.transform);
            heroController.gameObject.SetActive(true);
        }
        void InitializeSimpleHero()
        {
            GameObject simpleHeroGo = Instantiate(ResourcesManager.Instance.heroSimple);
            simpleHero = simpleHeroGo.GetComponentInChildren<SimpleHero>();

            simpleHero.Initialize();

            heroController.gameObject.SetActive(false);

            simpleHero.transform.position = ResourcesManager.Instance.simpleHeroSpawnPosition.position;
            simpleHero.transform.rotation = ResourcesManager.Instance.simpleHeroSpawnPosition.rotation;

            simpleHero.transform.SetParent(this.transform);
            simpleHero.gameObject.SetActive(true);
        }
    }
}