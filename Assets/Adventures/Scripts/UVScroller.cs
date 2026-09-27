using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    public class UVScroller : MonoBehaviour
    {
        public Vector2 speed = new Vector2(0.02f, 0.01f);
        public float bob = 0.05f;
        Material m; Vector3 start;
        void Start() { m = GetComponent<Renderer>().material; start = transform.position; }
        void Update()
        {
            m.SetTextureOffset("_BaseMap", speed * Time.time);
            transform.position = start + Vector3.up * Mathf.Sin(Time.time * 0.8f) * bob;
        }
    }
}
