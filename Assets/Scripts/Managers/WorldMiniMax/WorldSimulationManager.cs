using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace HOMM_BM
{
    public class WorldSimulationManager : MonoBehaviour
    {
        public static WorldSimulationManager instance;

        private int moveCount;

        HeroController currentHero;
        HeroController targetHero;

        List<HeroController> heroesQueue = new List<HeroController>();

        bool isInteractionInitialized;
        bool isTargetPointBlank;
        bool aiInteracting;
        public bool AiInteracting { get => aiInteracting; set => aiInteracting = value; }
        public int MoveCount { get => moveCount; set => moveCount = value; }

        public void Awake()
        {
            instance = this;
        }

        public void Initialize()
        {
            heroesQueue = new List<HeroController>(WorldManager.instance.HeroesQueue);
            currentHero = heroesQueue.First();

            WorldMiniMax minimax = new WorldMiniMax(heroesQueue, 1);
            HeroMove heroMove = minimax.StartMiniMax();

            MoveCount++;

            if (heroMove == null)
            {
                Debug.Log("Something went wrong!");
                return;
            }
            else
            {
                aiInteracting = true;
                if (heroMove.HeroAvailableFromNode != null)
                {
                    foreach (HeroController hero in heroesQueue)
                    {
                        if (heroMove.HeroAvailableFromNode.HeroId.Equals(hero.GetInstanceID()))
                        {
                            isInteractionInitialized = true;
                            targetHero = hero;
                        }
                    }
                }

                else if (currentHero.CurrentNode == heroMove.TargetNode)
                {
                    WorldManager.instance.OnMoveFinished();
                }

                else if (!isInteractionInitialized)
                {
                    PathfinderMaster.instance.RequestPathAndPreview(currentHero.CurrentNode,
                        heroMove.TargetNode, currentHero);

                    currentHero.IsInteractionInitialized = true;
                    currentHero.InitializeMoveToInteractionContainer(heroMove.TargetNode);
                }

                if (isInteractionInitialized)
                {
                    isInteractionInitialized = false;
                    HandeAiAction();

                    if (targetHero != null)
                    {
                        targetHero = null;
                    }
                }
            }
        }

        void HandeAiAction()
        {
            HeroInteractionHook ih = targetHero.GetComponentInChildren<HeroInteractionHook>();

            if (ih == null || !ih.enabled)
            {
                targetHero = null;
                return;
            }
            Node targetNode = GridManager.instance.GetNode(ih.interactionPoint.position, currentHero.GridIndex);

            PathfinderMaster.instance.RequestPathAndPreview(currentHero.CurrentNode,
                            targetNode, currentHero);

            currentHero.currentInteractionHook = ih;
            currentHero.IsInteractionInitialized = true;
            currentHero.CreateInteractionContainer(currentHero.currentInteractionHook.interactionContainer);
        }
    }
}