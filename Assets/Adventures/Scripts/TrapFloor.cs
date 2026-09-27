using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    /// A floor tile that collapses shortly after the player steps on it, then resets.
    public class TrapFloor : PlayerZone
    {
        public Transform tile;
        Vector3 home; Quaternion homeRot; bool busy;
        protected override void Awake() { base.Awake(); home = tile.position; homeRot = tile.rotation; }
        protected override void OnPlayerEnter(PlayerController p) { if (!busy) StartCoroutine(Collapse()); }
        IEnumerator Collapse()
        {
            busy = true;
            if (AudioManager.I) AudioManager.I.Play(Sfx.Rumble, 0.6f);
            if (ThirdPersonCamera.I) ThirdPersonCamera.I.Shake(0.08f, 0.4f);
            yield return new WaitForSeconds(0.3f);
            foreach (var c in tile.GetComponentsInChildren<Collider>()) c.enabled = false;
            float v = 0, t = 0;
            while (t < 1.5f) { t += Time.deltaTime; v += 15 * Time.deltaTime; tile.position += Vector3.down * v * Time.deltaTime; yield return null; }
            yield return new WaitForSeconds(1.5f);
            tile.SetPositionAndRotation(home, homeRot);
            foreach (var c in tile.GetComponentsInChildren<Collider>()) c.enabled = true;
            busy = false;
        }
    }
}
