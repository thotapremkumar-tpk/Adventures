using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    public class SceneExit : PlayerZone
    {
        public string sceneName;
        public string requiredItem;
        public string lockedMessage;
        bool used;
        protected override void OnPlayerEnter(PlayerController p)
        {
            if (used) return;
            if (!string.IsNullOrEmpty(requiredItem) && !GameState.Has(requiredItem)) { UIManager.I.Toast(lockedMessage); return; }
            used = true;
            GameState.InputLocked = true;
            UIManager.I.FadeOut(0.8f, () => { GameState.InputLocked = false; SceneManager.LoadScene(sceneName); });
        }
    }
}
