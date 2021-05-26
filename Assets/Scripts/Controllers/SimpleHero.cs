using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class SimpleHero : MonoBehaviour
    {
        public GameObject hero;
        public InventoryReference inventoryReference;

        public void Initialize()
        {
            hero = Instantiate(WorldManager.instance.currentHero.heroModel);
            hero.transform.SetParent(this.transform);
            hero.transform.localScale = Vector3.one;

            inventoryReference = WorldManager.instance.currentHero.InventoryReference;
        }
    }
}