using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    public class Spinner : MonoBehaviour
    {
        public float speed = 60f, bobHeight = 0.15f, bobSpeed = 2f;
        Vector3 start;
        void Start() { start = transform.localPosition; }
        void Update()
        {
            transform.Rotate(0, speed * Time.deltaTime, 0, Space.World);
            transform.localPosition = start + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        }
    }
}
