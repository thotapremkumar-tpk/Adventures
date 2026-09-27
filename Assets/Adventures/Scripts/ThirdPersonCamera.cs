using UnityEngine;
using UnityEngine.InputSystem;

namespace Adventures
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        public static ThirdPersonCamera I;
        public Transform target;
        public float distance = 4.5f, height = 1.6f, sensitivity = 0.12f;
        float yaw, pitch = 12f, shakeT, shakeAmp;
        const int Mask = ~(1 << 2);

        void Awake() { I = this; }
        void Start() { if (target) yaw = target.eulerAngles.y; }

        public void Shake(float amp, float dur) { shakeAmp = Mathf.Max(shakeAmp, amp); shakeT = Mathf.Max(shakeT, dur); }

        void LateUpdate()
        {
            if (!target) return;
            var m = Mouse.current; var kb = Keyboard.current;
            bool blocking = GameState.UIBlocking;
            if (m != null)
            {
                if (!blocking && m.leftButton.wasPressedThisFrame) Cursor.lockState = CursorLockMode.Locked;
                if (kb != null && kb.escapeKey.wasPressedThisFrame) Cursor.lockState = CursorLockMode.None;
                if (Cursor.lockState == CursorLockMode.Locked && !blocking)
                {
                    Vector2 d = m.delta.ReadValue();
                    yaw += d.x * sensitivity; pitch -= d.y * sensitivity;
                }
            }
            if (kb != null && !blocking)
            {
                if (kb.qKey.isPressed) yaw -= 90 * Time.deltaTime;
                if (kb.rKey.isPressed) yaw += 90 * Time.deltaTime;
            }
            pitch = Mathf.Clamp(pitch, -30f, 70f);
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
            Vector3 pivot = target.position + Vector3.up * height;
            Vector3 dir = rot * Vector3.back;
            float dist = distance;
            if (Physics.SphereCast(pivot, 0.25f, dir, out var hit, distance, Mask, QueryTriggerInteraction.Ignore))
                dist = Mathf.Max(0.6f, hit.distance - 0.05f);
            Vector3 pos = pivot + dir * dist;
            if (shakeT > 0) { shakeT -= Time.deltaTime; pos += Random.insideUnitSphere * shakeAmp * Mathf.Clamp01(shakeT * 2f); }
            else shakeAmp = 0;
            transform.SetPositionAndRotation(pos, rot);
        }
    }
}
