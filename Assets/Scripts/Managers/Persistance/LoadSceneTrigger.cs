using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class LoadSceneTrigger : MonoBehaviour
    {
        public string targetScene;
        public UnitItem unitItem;
        public int stackSize;
        private void OnTriggerEnter(Collider other)
        {
            GameReferencesManager.instance.PrepareInteractionUnit(unitItem, stackSize);
            GameReferencesManager.instance.LoadTargetScene(targetScene);
        }
    }
}