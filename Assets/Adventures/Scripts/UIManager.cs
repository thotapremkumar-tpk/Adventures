using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Adventures
{
    /// Builds the whole HUD at runtime: prompts, toasts, objective, inventory, story pages, map viewer, end panel, fades.
    public class UIManager : MonoBehaviour
    {
        public static UIManager I;
        Font font;
        Text prompt, toast, inventory, objective, storyTitle, storyBody, storyFooter, mapLegend, endTitle, endBody, help;
        GameObject storyPanel, mapPanel, endPanel, helpPanel;
        RawImage mapImage;
        Image fade;
        float toastTimer, inputGuard;
        List<StoryPage> pages; int pageIndex; Action onStoryDone, onMapClosed, onEnd;

        public static UIManager Ensure()
        {
            if (I == null) { var go = new GameObject("UIManager"); I = go.AddComponent<UIManager>(); }
            return I;
        }

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Build();
            GameState.UIBlocking = false;
            GameState.InputLocked = false;
        }

        RectTransform Rect(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = oMin; rt.offsetMax = oMax;
            return rt;
        }

        Text Txt(Transform parent, string name, int size, TextAnchor anchor, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, Color c)
        {
            var rt = Rect(parent, name, aMin, aMax, oMin, oMax);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = font; t.fontSize = size; t.alignment = anchor; t.color = c;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            var sh = rt.gameObject.AddComponent<Shadow>(); sh.effectDistance = new Vector2(2, -2); sh.effectColor = new Color(0, 0, 0, 0.8f);
            return t;
        }

        Image Img(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, Color c)
        {
            var rt = Rect(parent, name, aMin, aMax, oMin, oMax);
            var i = rt.gameObject.AddComponent<Image>(); i.color = c; return i;
        }

        void Build()
        {
            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            var root = canvasGo.transform;
            Color gold = new Color(1f, 0.85f, 0.5f);

            prompt = Txt(root, "Prompt", 34, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-600, 140), new Vector2(600, 200), Color.white);
            toast = Txt(root, "Toast", 36, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(-800, -230), new Vector2(800, -120), gold);
            inventory = Txt(root, "Inventory", 28, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -200), new Vector2(700, -20), Color.white);
            objective = Txt(root, "Objective", 28, TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-900, -200), new Vector2(-30, -20), new Color(0.8f, 0.95f, 1f));
            var hint = Txt(root, "Hint", 22, TextAnchor.LowerLeft, new Vector2(0, 0), new Vector2(0, 0), new Vector2(30, 20), new Vector2(1200, 60), new Color(1, 1, 1, 0.6f));
            hint.text = "WASD move · Shift sprint · Space jump · Mouse look (click) · E interact · F torch · M map · H help";

            // story panel
            var sp = Img(root, "StoryPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.72f));
            storyPanel = sp.gameObject;
            var card = Img(sp.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-720, -340), new Vector2(720, 340), new Color(0.16f, 0.12f, 0.08f, 0.96f));
            Img(card.transform, "Border", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -8), new Vector2(0, 0), gold);
            storyTitle = Txt(card.transform, "Title", 52, TextAnchor.UpperCenter, new Vector2(0, 1), new Vector2(1, 1), new Vector2(40, -110), new Vector2(-40, -30), gold);
            storyTitle.fontStyle = FontStyle.Bold;
            storyBody = Txt(card.transform, "Body", 32, TextAnchor.UpperLeft, new Vector2(0, 0), new Vector2(1, 1), new Vector2(70, 90), new Vector2(-70, -130), new Color(0.95f, 0.92f, 0.85f));
            storyBody.lineSpacing = 1.15f;
            storyFooter = Txt(card.transform, "Footer", 24, TextAnchor.LowerRight, new Vector2(0, 0), new Vector2(1, 0), new Vector2(40, 25), new Vector2(-40, 70), new Color(1, 1, 1, 0.6f));
            storyPanel.SetActive(false);

            // map panel
            var mp = Img(root, "MapPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.85f));
            mapPanel = mp.gameObject;
            var mi = Rect(mp.transform, "Map", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-860, -420), new Vector2(260, 420));
            mapImage = mi.gameObject.AddComponent<RawImage>();
            mapLegend = Txt(mp.transform, "Legend", 26, TextAnchor.UpperLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(300, -420), new Vector2(900, 420), new Color(1f, 0.93f, 0.8f));
            mapPanel.SetActive(false);

            // end panel
            var ep = Img(root, "EndPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.02f, 0.02f, 0.03f, 0.9f));
            endPanel = ep.gameObject;
            endTitle = Txt(ep.transform, "Title", 80, TextAnchor.MiddleCenter, new Vector2(0, 0.55f), new Vector2(1, 0.8f), Vector2.zero, Vector2.zero, gold);
            endTitle.fontStyle = FontStyle.Bold;
            endBody = Txt(ep.transform, "Body", 34, TextAnchor.UpperCenter, new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.55f), Vector2.zero, Vector2.zero, Color.white);
            endPanel.SetActive(false);

            // help
            var hp = Img(root, "Help", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-420, -260), new Vector2(420, 260), new Color(0, 0, 0, 0.85f));
            helpPanel = hp.gameObject;
            help = Txt(hp.transform, "Text", 30, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(40, 20), new Vector2(-40, -20), Color.white);
            help.text = "CONTROLS\n\nW A S D  – move\nShift  – sprint\nSpace  – jump\nMouse  – look (click the game to capture)\nE  – interact / pick up / open\nF  – torch on/off\nM  – view the treasure map\nTab  – re-read the level scroll\nEsc  – release mouse\nH  – close this help";
            helpPanel.SetActive(false);

            fade = Img(root, "Fade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.black);
            fade.raycastTarget = false;
            RefreshInventory();
            SetObjective("");
            SetPrompt(null);
            toast.text = "";
        }

        void Update()
        {
            GameState.UIBlocking = storyPanel.activeSelf || mapPanel.activeSelf || endPanel.activeSelf;
            if (toastTimer > 0) { toastTimer -= Time.deltaTime; var c = toast.color; c.a = Mathf.Clamp01(toastTimer); toast.color = c; }
            var kb = Keyboard.current; var mouse = Mouse.current;
            if (kb == null) return;
            if (Time.unscaledTime < inputGuard) return;
            bool next = kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            if (storyPanel.activeSelf && next) { NextPage(); return; }
            if (mapPanel.activeSelf && (next || kb.mKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)) { CloseMap(); return; }
            if (endPanel.activeSelf && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)) { endPanel.SetActive(false); onEnd?.Invoke(); return; }
            if (!GameState.UIBlocking)
            {
                if (kb.hKey.wasPressedThisFrame) helpPanel.SetActive(!helpPanel.activeSelf);
                if (kb.mKey.wasPressedThisFrame && GameState.Has("map") && MapData.Texture != null) ShowMap(MapData.Texture, MapData.Legend, null);
            }
        }

        public void SetPrompt(string s) { if (prompt) prompt.text = string.IsNullOrEmpty(s) ? "" : s; }
        public void Toast(string s, float seconds = 4f) { toast.text = s; toastTimer = seconds; var c = toast.color; c.a = 1; toast.color = c; }
        public void SetObjective(string s) { objective.text = string.IsNullOrEmpty(s) ? "" : "OBJECTIVE\n" + s; }

        public void RefreshInventory()
        {
            if (!inventory) return;
            var items = GameState.Items.Select(GameState.DisplayName).ToList();
            inventory.text = items.Count == 0 ? "" : "INVENTORY\n• " + string.Join("\n• ", items);
        }

        public void ShowStory(List<StoryPage> p, Action done)
        {
            if (p == null || p.Count == 0) { done?.Invoke(); return; }
            pages = p; pageIndex = 0; onStoryDone = done;
            storyPanel.SetActive(true); GameState.UIBlocking = true;
            Cursor.lockState = CursorLockMode.None;
            ShowPage();
        }

        void ShowPage()
        {
            storyTitle.text = pages[pageIndex].title;
            storyBody.text = pages[pageIndex].body;
            storyFooter.text = (pageIndex + 1) + " / " + pages.Count + "    Press Enter / Space / E to continue";
            inputGuard = Time.unscaledTime + 0.25f;
            if (AudioManager.I) AudioManager.I.Play(Sfx.Page, 0.5f);
        }

        void NextPage()
        {
            pageIndex++;
            if (pageIndex >= pages.Count)
            {
                storyPanel.SetActive(false); GameState.UIBlocking = false;
                var d = onStoryDone; onStoryDone = null; d?.Invoke();
                inputGuard = Time.unscaledTime + 0.25f;
            }
            else ShowPage();
        }

        public void ShowMap(Texture tex, string legend, Action closed)
        {
            mapImage.texture = tex; mapLegend.text = legend; onMapClosed = closed;
            mapPanel.SetActive(true); GameState.UIBlocking = true; inputGuard = Time.unscaledTime + 0.3f;
            if (AudioManager.I) AudioManager.I.Play(Sfx.Page, 0.6f);
        }

        void CloseMap()
        {
            mapPanel.SetActive(false); GameState.UIBlocking = false; inputGuard = Time.unscaledTime + 0.25f;
            var c = onMapClosed; onMapClosed = null; c?.Invoke();
        }

        public void ShowEnd(string title, string body, Action confirm)
        {
            endTitle.text = title; endBody.text = body + "\n\n<size=26>Press Enter to continue</size>";
            endBody.supportRichText = true; onEnd = confirm;
            endPanel.SetActive(true); GameState.UIBlocking = true; Cursor.lockState = CursorLockMode.None;
            inputGuard = Time.unscaledTime + 0.8f;
        }

        public void FadeIn(float t) { StartCoroutine(FadeRoutine(1, 0, t, null)); }
        public void FadeOut(float t, Action done) { StartCoroutine(FadeRoutine(fade.color.a, 1, t, done)); }

        IEnumerator FadeRoutine(float a, float b, float t, Action done)
        {
            float e = 0;
            while (e < t) { e += Time.unscaledDeltaTime; var c = fade.color; c.a = Mathf.Lerp(a, b, e / t); fade.color = c; yield return null; }
            var cc = fade.color; cc.a = b; fade.color = cc;
            done?.Invoke();
        }
    }

    /// Holds the treasure map texture for the M key.
    public static class MapData
    {
        public static Texture Texture;
        public static string Legend;
    }
}
