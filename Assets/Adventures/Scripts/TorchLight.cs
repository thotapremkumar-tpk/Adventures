using UnityEngine;
using UnityEngine.InputSystem;

namespace Adventures
{
    /// The archaeologist's big torch: spot light that follows where the camera looks.
    public class TorchLight : MonoBehaviour
    {
        public Light spot, fill;
        public Transform hand;
        bool on = true; float baseI;

        void Start() { baseI = spot ? spot.intensity : 1f; }

        void LateUpdate()
        {
            var cam = Camera.main;
            if (spot)
            {
                Vector3 f = cam ? Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized : transform.forward;
                spot.transform.position = transform.position + Vector3.up * 1.75f + f * 0.6f;
                if (cam) spot.transform.rotation = Quaternion.Slerp(spot.transform.rotation, cam.transform.rotation * Quaternion.Euler(6, 0, 0), 1 - Mathf.Exp(-12 * Time.deltaTime));
            }
            var kb = Keyboard.current;
            if (kb != null && kb.fKey.wasPressedThisFrame && !GameState.UIBlocking)
            {
                on = !on; if (AudioManager.I) AudioManager.I.Play(Sfx.Page, 0.4f);
            }
            if (spot) { spot.enabled = on; spot.intensity = baseI * (0.94f + 0.06f * Mathf.PerlinNoise(Time.time * 4f, 0)); }
            if (fill) fill.enabled = on;
        }
    }
}
