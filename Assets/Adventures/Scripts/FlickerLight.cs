using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    [RequireComponent(typeof(Light))]
    public class FlickerLight : MonoBehaviour
    {
        public float amount = 0.25f, speed = 6f;
        Light l; float baseI, seed;
        void Start() { l = GetComponent<Light>(); baseI = l.intensity; seed = Random.value * 100; }
        void Update() { l.intensity = baseI * (1 - amount + amount * 2 * Mathf.PerlinNoise(seed, Time.time * speed)); }
    }
}
