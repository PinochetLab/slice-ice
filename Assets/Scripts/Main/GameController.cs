using System.Collections;
using Levels;
using UIs;
using UnityEngine;
using Zenject;

namespace Main
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private Level level;
        [SerializeField] private WhiteScreen whiteScreen;
        
        [Inject] private IceController _iceController;

        private void Start()
        {
            Debug.Log("Generate level");
            _iceController.GenerateLevel(level);
        }

        public void Retry()
        {
            StartCoroutine(RetryCoroutine());
        }

        private IEnumerator RetryCoroutine()
        {
            yield return whiteScreen.Show();
            _iceController.ClearLevel();
            _iceController.GenerateLevel(level);
            yield return whiteScreen.Hide();
        }
    }
}