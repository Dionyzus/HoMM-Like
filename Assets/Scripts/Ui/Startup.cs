using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HOMM_BM
{
    public class Startup : MonoBehaviour
    {
        [SerializeField] AudioSource logoSound = default;
        void Start()
        {
            logoSound.Play();
            StartCoroutine(TakeToMenu());
        }
        IEnumerator TakeToMenu()
        {
            yield return new WaitForSeconds(4.5f);
            SceneManager.LoadScene(1);
        }
    }
}