using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    public class InteractWithHeroInteraction : Interaction
    {
        DialogPrompt dialogPrompt;
        float timer;
        bool dialogInitialized;

        string targetScene;
        HeroInteractionHook heroInteractionHook;

        public InteractWithHeroInteraction(string targetScene, HeroInteractionHook heroInteractionHook)
        {
            this.targetScene = targetScene;
            this.heroInteractionHook = heroInteractionHook;
        }

        public override void OnEnd(GridUnit gridUnit)
        {
            if (gridUnit.gameObject.layer == GridManager.ENEMY_UNITS_LAYER)
            {
                AiInitiatedBattle(gridUnit);
            }
            else
            {
                gridUnit.ClearInteractionData();
                gridUnit.ActionIsDone();
            }
        }

        void AiInitiatedBattle(GridUnit gridUnit)
        {
            HeroController heroController = (HeroController)gridUnit;
            heroController.ClearInteractionData();

            HeroInteractionHook hook = heroController.GetComponentInChildren<HeroInteractionHook>();

            GameReferencesManager.instance.PrepareInteractionWithHero(hook.Items);
            GameReferencesManager.instance.LoadTargetScene(targetScene);
        }

        void EnterTheBattle(HeroController heroController, string targetScene)
        {
            heroController.ClearInteractionData();

            GameReferencesManager.instance.PrepareInteractionWithHero(heroInteractionHook.Items);
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
                if (gridUnit.gameObject.layer == GridManager.FRIENDLY_UNITS_LAYER)
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
                else
                {
                    return true;
                }
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