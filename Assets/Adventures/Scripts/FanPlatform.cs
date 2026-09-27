using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    /// Level 1 fan: spins continuously and, on a timer, moves up and back toward the tower, holds, then returns.
    [DefaultExecutionOrder(-50)]
    public class FanPlatform : MonoBehaviour
    {
        public Transform spinner;
        public float spinSpeed = 40f;
        public Vector3 moveOffset = new Vector3(0, 0.5f, 0);
        public float holdTime = 1.6f, moveTime = 1.0f, phase;
        Vector3 basePos;
        void Awake() { basePos = transform.localPosition; }
        static float S(float x) => x * x * (3 - 2 * x);
        void Update()
        {
            float cycle = 2 * (holdTime + moveTime);
            float t = Mathf.Repeat(Time.time + phase, cycle), s;
            if (t < holdTime) s = 0;
            else if (t < holdTime + moveTime) s = S((t - holdTime) / moveTime);
            else if (t < 2 * holdTime + moveTime) s = 1;
            else s = 1 - S((t - 2 * holdTime - moveTime) / moveTime);
            transform.localPosition = basePos + moveOffset * s;
            if (spinner) spinner.Rotate(0, spinSpeed * Time.deltaTime, 0, Space.Self);
        }
    }
}
