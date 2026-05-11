using System;
using System.Collections;
using UnityEngine;

namespace UIs
{
    public class WhiteScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float duration = 0.3f;

        private const int N = 30;

        private void Awake()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }

        public IEnumerator Show()
        {
            canvasGroup.blocksRaycasts = true;
            for (var i = 0; i <= N; i++)
            {
                var alpha = (float)i / N;
                canvasGroup.alpha = alpha;
                if (i < N)
                    yield return new WaitForSeconds(duration / 2 / N);
            }
        }
        
        public IEnumerator Hide()
        {
            for (var i = 0; i <= N; i++)
            {
                var alpha = 1f - (float)i / N;
                canvasGroup.alpha = alpha;
                if (i < N)
                    yield return new WaitForSeconds(duration / 2 / N);
            }
            canvasGroup.blocksRaycasts = false;
        }
    }
}