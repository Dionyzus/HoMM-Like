using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Mouse Logic/Select and Move Hero")]
    public class SelectAndMoveHero : MouseLogicWorld
    {
        public override void InteractTick(WorldManager worldManager, RaycastHit hit)
        {
            InteractionHook ih = hit.transform.GetComponentInParent<InteractionHook>();
            if (ih)
                return;

            ISelectable selectable = hit.transform.GetComponentInParent<ISelectable>();
            if (selectable != null && selectable.GetGridUnit().gameObject.layer != GridManager.ENEMY_UNITS_LAYER)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (selectable.GetGridUnit() != worldManager.currentHero && selectable.GetType() == typeof(HeroController))
                    {
                        worldManager.currentHero = (HeroController)selectable.GetGridUnit();
                        UiManager.instance.OnHeroSelected(worldManager.currentHero);
                    }
                }
            }
            else
            {
                if (worldManager.currentHero != null)
                {
                    Node targetNode = null;

                    if (Input.GetMouseButtonDown(0))
                    {
                        targetNode = GridManager.instance.GetNode(hit.point, worldManager.currentHero.gridIndex);

                        if (worldManager.currentHero.IsInteractionPointBlank)
                        {
                            worldManager.currentHero.IsInteractionPointBlank = false;
                        }
                        worldManager.currentHero.PreviewPathToNode(targetNode);
                    }

                    if (PathfinderMaster.instance.StoredPath.Count > 0 && Input.GetKeyDown(KeyCode.Space))
                    {
                        worldManager.currentHero.IsInteractionInitialized = true;
                        worldManager.currentHero.InitializeMoveToInteractionContainer(targetNode);
                    }
                }
            }
        }

        public override void InteractTick(WorldManager worldManager, HeroController heroController)
        {
            ISelectable selectable = heroController.GetComponentInParent<ISelectable>();

            if (selectable != null)
            {
                if (selectable.GetGridUnit() != worldManager.currentHero)
                {
                    worldManager.currentHero = (HeroController)selectable.GetGridUnit();
                }
            }
        }
    }
}