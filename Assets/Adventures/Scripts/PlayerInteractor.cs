using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    public class PlayerInteractor : MonoBehaviour
    {
        void Update()
        {
            var ui = UIManager.I; if (!ui) return;
            if (GameState.UIBlocking || GameState.InputLocked) { ui.SetPrompt(null); return; }
            Interactable best = null; float bestD = float.MaxValue;
            Vector3 me = transform.position + Vector3.up;
            foreach (var it in All())
            {
                if (!it || !it.isActiveAndEnabled || !it.CanInteract) continue;
                float d = Vector3.Distance(me, it.transform.position);
                if (d < it.range && d < bestD) { bestD = d; best = it; }
            }
            ui.SetPrompt(best ? "[E]  " + best.prompt : null);
            var kb = Keyboard.current;
            if (best && kb != null && kb.eKey.wasPressedThisFrame) best.Interact(PlayerController.I);
        }
        static List<Interactable> All() => Interactable.All.ToList();
    }
}
