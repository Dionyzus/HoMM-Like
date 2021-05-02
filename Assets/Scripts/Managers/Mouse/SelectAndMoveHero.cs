using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [CreateAssetMenu(menuName = "Mouse Logic/Select and Move Hero")]
    public class SelectAndMoveHero : MouseLogicWorld
    {
        GameObject hitLookAt;
        public override void InteractTick(WorldManager worldManager, RaycastHit hit)
        {
            InteractionHook ih = hit.transform.GetComponentInParent<InteractionHook>();
            if (ih)
                return;

            ISelectable selectable = hit.transform.GetComponentInParent<ISelectable>();
            if (selectable != null && selectable.GetGridUnit().gameObject.layer != GridManager.ENEMY_UNITS_LAYER)
            {
                if (GameManager.instance.Mouse.leftButton.isPressed)
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

                    if (GameManager.instance.Mouse.leftButton.isPressed)
                    {
                        targetNode = GridManager.instance.GetNode(hit.point, worldManager.currentHero.GridIndex);

                        if (worldManager.currentHero.IsInteractionPointBlank)
                        {
                            worldManager.currentHero.IsInteractionPointBlank = false;
                        }
                        if (targetNode != null)
                            worldManager.currentHero.PreviewPathToNode(targetNode);
                    }

                    if (PathfinderMaster.instance.StoredPath.Count > 0 && GameManager.instance.Keyboard.spaceKey.isPressed)
                    {
                        worldManager.currentHero.IsInteractionInitialized = true;

                        if (hitLookAt != null)
                            Destroy(hitLookAt);

                        hitLookAt = Instantiate(worldManager.hitLookAtPrefab);
                        hitLookAt.transform.position = hit.point;
                        hitLookAt.SetActive(true);

                        worldManager.ActivateLookAtActionCamera(hitLookAt.transform);
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