using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    public class TreasureChest : Interactable
    {
        public string title = "LEVEL 2 COMPLETE";
        [TextArea] public string body;
        public string nextScene;
        public Transform lid;
        public GameObject glow;
        bool done;
        public override bool CanInteract => !done;

        public override void Interact(PlayerController p)
        {
            done = true;
            if (AudioManager.I) AudioManager.I.Play(Sfx.Success);
            if (glow) glow.SetActive(true);
            if (lid) lid.localRotation = Quaternion.Euler(-70, 0, 0) * lid.localRotation;
            StartCoroutine(End());
        }

        IEnumerator End()
        {
            yield return new WaitForSeconds(1.5f);
            UIManager.I.ShowEnd(title, body, () => { if (!string.IsNullOrEmpty(nextScene)) { GameState.Items.Clear(); SceneManager.LoadScene(nextScene); } });
        }
    }
}
