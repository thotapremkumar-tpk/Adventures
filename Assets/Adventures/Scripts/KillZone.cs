using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    public class KillZone : PlayerZone
    {
        public string message = "You fell!";
        public Sfx sound = Sfx.Splash;
        bool busy;
        protected override void OnPlayerEnter(PlayerController p)
        {
            if (busy) return;
            busy = true;
            if (AudioManager.I) AudioManager.I.Play(sound);
            UIManager.I.Toast(message, 3f);
            GameState.InputLocked = true;
            UIManager.I.FadeOut(0.35f, () => { p.Respawn(); GameState.InputLocked = false; UIManager.I.FadeIn(0.5f); busy = false; });
        }
    }
}
