using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class LoadSceneTrigger : MonoBehaviour
    {
        public string targetScene;

        private void OnTriggerEnter(Collider other)
        {
            GameReferencesManager.instance.LoadTargetScene(targetScene);
        }
    }
}