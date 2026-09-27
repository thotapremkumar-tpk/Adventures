using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    /// Stone door that slides into the ground when the player has the required keys.
    public class LockedDoor : Interactable
    {
        public string[] requiredItems;
        public Transform slab;
        public float openDepth = 4f;
        public List<StoryPage> openStory = new List<StoryPage>();
        public string newObjective;
        bool opened;

        public override bool CanInteract => !opened;

        public override void Interact(PlayerController p)
        {
            var missing = requiredItems.Where(i => !GameState.Has(i)).ToList();
            if (missing.Count > 0)
            {
                if (AudioManager.I) AudioManager.I.Play(Sfx.Fail);
                UIManager.I.Toast("The door is sealed. It needs: " + string.Join(" + ", missing.Select(GameState.DisplayName)));
                return;
            }
            opened = true;
            UIManager.I.ShowStory(openStory, () => { Cursor.lockState = CursorLockMode.Locked; StartCoroutine(Open()); });
        }

        IEnumerator Open()
        {
            if (AudioManager.I) AudioManager.I.Play(Sfx.DoorGrind);
            if (ThirdPersonCamera.I) ThirdPersonCamera.I.Shake(0.05f, 2.5f);
            Vector3 a = slab.position, b = a + Vector3.down * openDepth;
            float t = 0;
            while (t < 2.5f) { t += Time.deltaTime; slab.position = Vector3.Lerp(a, b, Mathf.SmoothStep(0, 1, t / 2.5f)); yield return null; }
            foreach (var c in slab.GetComponentsInChildren<Collider>()) c.enabled = false;
            if (!string.IsNullOrEmpty(newObjective)) UIManager.I.SetObjective(newObjective);
        }
    }
}
