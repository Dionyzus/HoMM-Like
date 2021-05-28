using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Resources Manager/Resources Manager Asset")]
    public class ResourcesManagerAsset : ScriptableObject
    {
        public DialogPrompt enterBattleDialog;

        public GameObject heroController;
        public Transform heroControllerSpawnPosition;

        public GameObject heroSimple;
        public Transform simpleHeroSpawnPosition;

        //This could be used later for bigger world, to separate big world into few smaller ones 
        //public GameObject sceneTrigger;
        //public Transform sceneTriggerSpawnPosition;

        [SerializeField]
        InteractionHookTransformDictionary interactions = new InteractionHookTransformDictionary();

        [SerializeField]
        StringUnitControllerDictionary units = new StringUnitControllerDictionary();

        public StringUnitControllerDictionary Units { get => units; set => units = value; }
        public InteractionHookTransformDictionary Interactions { get => interactions; set => interactions = value; }
    }
}