using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    public class TriggerMessage : PlayerZone
    {
        [TextArea] public string toast;
        public List<StoryPage> story = new List<StoryPage>();
        public string newObjective;
        public bool once = true;
        public bool playSound;
        bool done;
        protected override void OnPlayerEnter(PlayerController p)
        {
            if (done && once) return;
            done = true;
            if (playSound && AudioManager.I) AudioManager.I.Play(Sfx.Pickup, 0.5f);
            if (!string.IsNullOrEmpty(toast)) UIManager.I.Toast(toast, 5f);
            if (story.Count > 0) UIManager.I.ShowStory(story, () => Cursor.lockState = CursorLockMode.Locked);
            if (!string.IsNullOrEmpty(newObjective)) UIManager.I.SetObjective(newObjective);
        }
    }
}
