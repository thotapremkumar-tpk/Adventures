using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    /// Picks up one or more items; can show the treasure map and story pages.
    public class PickupItem : Interactable
    {
        public string[] itemIds;
        public string toast;
        public List<StoryPage> story = new List<StoryPage>();
        public Texture2D mapTexture;
        [TextArea] public string mapLegend;
        public string newObjective;

        public override void Interact(PlayerController p)
        {
            foreach (var id in itemIds) GameState.Give(id);
            if (AudioManager.I) AudioManager.I.Play(Sfx.Pickup);
            if (!string.IsNullOrEmpty(toast)) UIManager.I.Toast(toast);
            var obj = newObjective;
            System.Action afterStory = () => { if (!string.IsNullOrEmpty(obj)) UIManager.I.SetObjective(obj); Cursor.lockState = CursorLockMode.Locked; };
            if (mapTexture)
            {
                MapData.Texture = mapTexture; MapData.Legend = mapLegend;
                var st = story;
                UIManager.I.ShowMap(mapTexture, mapLegend, () => UIManager.I.ShowStory(st, afterStory));
            }
            else if (story.Count > 0) UIManager.I.ShowStory(story, afterStory);
            else afterStory();
            gameObject.SetActive(false);
        }
    }
}
