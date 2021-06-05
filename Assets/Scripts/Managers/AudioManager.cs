using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HOMM_BM
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] AudioClip[] audioClips = default;
        [SerializeField] float delayBetweenClips = 0f;
        [SerializeField] bool loopAudio = false;

        bool canPlay;
        AudioSource audioSource;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            canPlay = true;
        }

        public void Play()
        {
            if (!canPlay)
            {
                return;
            }
            GameManager.instance.TimeManager.Add(() =>
            {
                canPlay = true;
            }, delayBetweenClips);

            canPlay = false;

            int clipIndex = Random.Range(0, audioClips.Length);
            AudioClip audioClip = audioClips[clipIndex];

            if (loopAudio)
            {
                audioSource.loop = true;
                audioSource.clip = audioClip;
                audioSource.Play();
            }
            else
                audioSource.PlayOneShot(audioClip);
        }

        public void Stop()
        {
            audioSource.Stop();
        }
    }
}