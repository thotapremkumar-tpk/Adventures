using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    /// Per-scene setup: fade in, ambient audio, intro story, objective, dev shortcuts.
    public class LevelIntro : MonoBehaviour
    {
        public string levelName;
        public List<StoryPage> pages = new List<StoryPage>();
        public string objective;
        public string[] grantIfMissing = new string[0];
        public bool rereadWithTab;
        public float ambientRumble = 0.5f;
        public bool drips = true;
        public string nextScene;
        public Texture2D mapTexture;
        [TextArea] public string mapLegend;

        void Start()
        {
            UIManager.Ensure(); AudioManager.Ensure();
            GameState.InputLocked = false;
            foreach (var id in grantIfMissing) if (!GameState.Has(id)) GameState.Give(id);
            UIManager.I.RefreshInventory();
            if (mapTexture) { MapData.Texture = mapTexture; MapData.Legend = mapLegend; }
            UIManager.I.FadeIn(1.2f);
            if (ambientRumble > 0) AudioManager.I.StartAmbient(ambientRumble, drips);
            if (!string.IsNullOrEmpty(levelName)) UIManager.I.Toast(levelName, 4f);
            if (pages.Count > 0) UIManager.I.ShowStory(pages, () => { UIManager.I.SetObjective(objective); Cursor.lockState = CursorLockMode.Locked; });
            else UIManager.I.SetObjective(objective);
        }

        void Update()
        {
            var kb = Keyboard.current; if (kb == null) return;
            if (rereadWithTab && kb.tabKey.wasPressedThisFrame && !GameState.UIBlocking) UIManager.I.ShowStory(pages, null);
            // developer shortcut: F5 skips to the next scene
            if (kb.f5Key.wasPressedThisFrame && !string.IsNullOrEmpty(nextScene)) SceneManager.LoadScene(nextScene);
        }
    }
}
