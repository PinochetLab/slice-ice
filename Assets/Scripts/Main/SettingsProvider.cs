using UnityEngine;

namespace Main
{
    public class SettingsProvider : MonoBehaviour
    {
        [SerializeField] private float thickness = 0.7f;
        [SerializeField] private float hideDepth = 0.4f;
        [SerializeField] private float hideSpeed = 1f;
        
        public float Thickness => thickness;
        public float HideDepth => hideDepth;
        public float HideSpeed => hideSpeed;
    }
}