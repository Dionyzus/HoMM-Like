using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class SceneTriggerInteraction : Interaction
    {
        DialogPrompt dialogPrompt;
        float timer;
        bool dialogInitialized;

        string targetScene;
        UnitItem unitItem;
        int stackSize;

        public SceneTriggerInteraction(string targetScene, UnitItem unitItem, int stackSize)
        {
            this.targetScene = targetScene;
            this.unitItem = unitItem;
            this.stackSize = stackSize;
        }

        public override void OnEnd(GridUnit gridUnit)
        {
            gridUnit.ClearInteractionData();
            gridUnit.ActionIsDone();
        }

        void EnterTheBattle(HeroController heroController, string targetScene)
        {
            InteractionHook interactionHook = SceneStateHandler.instance.InteractionHooks
                    .Find(hook => hook.GetInstanceID() == heroController.currentInteractionHook.GetInstanceID());

            SceneStateHandler.instance.UpdateActiveState(interactionHook.transform.name);
            heroController.ClearInteractionData();

            GameReferencesManager.instance.PrepareInteractionUnit(unitItem, stackSize);
            GameReferencesManager.instance.LoadTargetScene(targetScene);
        }
        public override bool TickIsFinished(GridUnit gridUnit, float deltaTime)
        {
            timer -= deltaTime;

            if (!dialogInitialized)
            {
                Vector3 direction = (gridUnit.currentInteractionHook.transform.position - gridUnit.transform.position).normalized;
                direction.y = 0;
                Quaternion rotation = Quaternion.LookRotation(direction);
                gridUnit.transform.rotation = Quaternion.Slerp(gridUnit.transform.rotation, rotation, deltaTime / .3f);
            }

            if (timer <= 0)
            {
                if (!dialogInitialized)
                {
                    HeroController heroController = (HeroController)gridUnit;
                    dialogPrompt = heroController.ReallyEnterTheBattlePrompt;

                    dialogPrompt.Show();
                    dialogPrompt.OnYesEvent += () => EnterTheBattle(heroController, targetScene);
                    dialogPrompt.OnNoEvent += () => OnEnd(heroController);

                    dialogInitialized = true;
                }
                if (!dialogPrompt.gameObject.activeSelf)
                    return true;
            }
            return false;
        }

        protected override void OnStart(GridUnit gridUnit)
        {
            timer = 0.5f;
            gridUnit.PlayAnimation(gridUnit.actionAnimation);
            gridUnit.Animator.SetBool("isInteracting", true);
        }
    }
}