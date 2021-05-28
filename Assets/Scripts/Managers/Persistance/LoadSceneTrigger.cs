using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class LoadSceneTrigger : MonoBehaviour
    {
        //Will be used with bigger map to separate it into several smaller scenes
        public string targetScene;
        private void OnTriggerEnter(Collider other)
        {
            GameReferencesManager.instance.LoadTargetScene(targetScene);
        }
    }
}