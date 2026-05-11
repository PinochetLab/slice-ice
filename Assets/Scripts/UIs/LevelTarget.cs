using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIs
{
    public class LevelTarget : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text percentText;
        [SerializeField] private Image sealLock;

        private float _score;

        private void Start()
        {
            Apply();
        }

        public void Add(float score)
        {
            _score += score;
            Apply();
        }

        private void Apply()
        {
            slider.value = _score;
            var percent = Mathf.FloorToInt(_score * 100f);
            percentText.text = $"{percent}%";
        }

        public void UnlockSeal()
        {
            sealLock.gameObject.SetActive(false);
        }
    }
}