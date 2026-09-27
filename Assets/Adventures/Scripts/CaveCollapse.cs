using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    /// The weak cave floor: cutscene shake, then the ground gives way and the player falls underground.
    public class CaveCollapse : PlayerZone
    {
        public Transform floorSlab;
        bool done;
        protected override void OnPlayerEnter(PlayerController p)
        {
            if (done) return;
            done = true;
            StartCoroutine(Run());
        }
        IEnumerator Run()
        {
            GameState.InputLocked = true;
            UIManager.I.Toast("The ground is cracking beneath your feet...!", 3f);
            if (AudioManager.I) AudioManager.I.Play(Sfx.Rumble);
            if (ThirdPersonCamera.I) ThirdPersonCamera.I.Shake(0.25f, 2.0f);
            yield return new WaitForSeconds(1.1f);
            foreach (var c in floorSlab.GetComponentsInChildren<Collider>()) c.enabled = false;
            if (AudioManager.I) AudioManager.I.Play(Sfx.Rumble, 1f);
            float v = 0; float t = 0;
            GameState.InputLocked = false;
            while (t < 3f) { t += Time.deltaTime; v += 18f * Time.deltaTime; floorSlab.position += Vector3.down * v * Time.deltaTime; floorSlab.Rotate(20 * Time.deltaTime, 0, 8 * Time.deltaTime); yield return null; }
        }
    }
}
