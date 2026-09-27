using System;
using System.Collections.Generic;
using UnityEngine;

namespace Adventures
{
    [Serializable]
    public class StoryPage
    {
        public string title;
        [TextArea(3, 10)] public string body;
        public StoryPage() { }
        public StoryPage(string t, string b) { title = t; body = b; }
    }

    public static class GameState
    {
        public static readonly HashSet<string> Items = new HashSet<string>();
        public static bool InputLocked;
        public static bool UIBlocking;

        public static bool Has(string id) => Items.Contains(id);
        public static void Give(string id) { Items.Add(id); if (UIManager.I) UIManager.I.RefreshInventory(); }
        public static void Take(string id) { Items.Remove(id); if (UIManager.I) UIManager.I.RefreshInventory(); }

        public static string DisplayName(string id)
        {
            switch (id)
            {
                case "key_sun": return "Sun Key";
                case "key_moon": return "Moon Key";
                case "map": return "Old Map";
                default: return id;
            }
        }
    }
}
