using System;
using UnityEngine;

namespace Environment
{
    public class LightRotator : MonoBehaviour
    {
        [SerializeField] private float speed = 50;

        private void Update()
        {
            transform.localEulerAngles += Vector3.up * (speed * Time.deltaTime);
        }
    }
}