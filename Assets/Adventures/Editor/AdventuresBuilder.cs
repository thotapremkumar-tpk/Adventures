using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Adventures;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// Builds all demo scenes procedurally from the imported CC0 assets + generated meshes/textures.
/// Menu: Adventures > Build All Scenes. Also runs automatically when Assets/Adventures/BUILD_REQUEST.txt exists.
[InitializeOnLoad]
public static partial class AdventuresBuilder
{
    const string Root = "Assets/Adventures";
    const string Gen = Root + "/Generated";
    const string ScenesDir = Root + "/Scenes";
    const string KayDir = "Assets/ThirdParty/KayKitDungeon";
    const string CharFbx = "Assets/ThirdParty/KayKitCharacters/Rogue_Hooded.fbx";
    const string RequestFile = Root + "/BUILD_REQUEST.txt";

    public const string SIntro = "00_Intro", SCave = "01_OldCave", SL1 = "02_Level1_FanTower", SL2 = "03_Level2_Maze";

    static AdventuresBuilder() { EditorApplication.delayCall += CheckRequest; }

    static void CheckRequest()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        if (!File.Exists(RequestFile)) return;
        File.Delete(RequestFile);
        if (File.Exists(RequestFile + ".meta")) File.Delete(RequestFile + ".meta");
        BuildAll();
    }

    [MenuItem("Adventures/Build All Scenes")]
    public static void BuildAll()
    {
        try
        {
            Debug.Log("[ADV] Build started");
            Directory.CreateDirectory(Gen); Directory.CreateDirectory(ScenesDir);
            Directory.CreateDirectory(Gen + "/Materials"); Directory.CreateDirectory(Gen + "/Meshes"); Directory.CreateDirectory(Gen + "/Textures");
            AssetDatabase.Refresh();
            matCache.Clear();
            Random.InitState(1234);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PrepareAssets();
            Step("Intro", 0.1f); BuildIntro();
            Step("Cave", 0.3f); BuildCave();
            Step("Level 1", 0.55f); BuildLevel1();
            Step("Level 2", 0.8f); BuildLevel2();
            EditorBuildSettings.scenes = new[] { SIntro, SCave, SL1, SL2 }.Select(s => new EditorBuildSettingsScene($"{ScenesDir}/{s}.unity", true)).ToArray();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene($"{ScenesDir}/{SIntro}.unity");
            Debug.Log("[ADV] Build finished OK");
        }
        catch (Exception e) { Debug.LogError("[ADV] Build failed: " + e); }
        finally { EditorUtility.ClearProgressBar(); }
    }

    static void Step(string s, float p) { EditorUtility.DisplayProgressBar("Adventures demo", "Building " + s + "...", p); Debug.Log("[ADV] Building " + s); }

    // ------------------------------------------------------------------ assets
    static Material kayMat, charMat, rockMat, rockDarkMat, stoneMat, brassMat, acidMat, waterMat, parchMat, goldMat, silverMat, particleMat, blackMat, crackMat;
    static Texture2D mapTex;
    static VolumeProfile volumeProfile;
    static RuntimeAnimatorController playerAnim;
    static Mesh[] rockMeshes; static Mesh stalactiteMesh, halfDiscMesh;
    static float kScale = 1f; static float wallH = 4f;

    static void PrepareAssets()
    {
        var kayTex = AssetDatabase.LoadAssetAtPath<Texture2D>(KayDir + "/dungeon_texture.png");
        kayMat = Mat("KayKitDungeon", Color.white, kayTex, 0.1f);
        var charTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/KayKitCharacters/rogue_texture.png");
        charMat = Mat("Archaeologist", Color.white, charTex, 0.2f);

        var rockTex = Tex("rock", 512, (x, y) => { float n = Fbm(x * 0.012f, y * 0.012f, 5); float c = Mathf.Abs(Fbm(x * 0.03f + 7, y * 0.03f, 3) - 0.5f) < 0.02f ? 0.55f : 1f; float v = Mathf.Lerp(0.18f, 0.42f, n) * c; return new Color(v * 1.05f, v * 0.95f, v * 0.85f); });
        rockMat = Mat("CaveRock", Color.white, rockTex, 0.18f, tiling: new Vector2(1, 1));
        rockDarkMat = Mat("CaveRockDark", new Color(0.6f, 0.58f, 0.55f), rockTex, 0.35f);
        var stoneTex = Tex("stoneblocks", 512, (x, y) => { int bx = (x + ((y / 64) % 2) * 64) % 128, by = y % 64; bool mortar = bx < 4 || by < 4; float n = Fbm(x * 0.02f, y * 0.02f, 4); float v = mortar ? 0.12f : Mathf.Lerp(0.3f, 0.5f, n); return new Color(v, v * 0.95f, v * 0.85f); });
        stoneMat = Mat("AncientStone", Color.white, stoneTex, 0.15f, tiling: new Vector2(2, 2));
        var metalTex = Tex("bronze", 256, (x, y) => { float n = Fbm(x * 0.05f, y * 0.05f, 4); return Color.Lerp(new Color(0.35f, 0.22f, 0.1f), new Color(0.2f, 0.4f, 0.32f), Mathf.Clamp01(n * 1.6f - 0.5f)); });
        brassMat = Mat("AncientBronze", Color.white, metalTex, 0.45f, metallic: 0.6f);
        var acidTex = Tex("acid", 256, (x, y) => { float n = Fbm(x * 0.04f, y * 0.04f, 4); return Color.Lerp(new Color(0.05f, 0.35f, 0.02f), new Color(0.5f, 1f, 0.2f), Mathf.Pow(n, 2f)); });
        acidMat = Mat("Acid", Color.white, acidTex, 0.9f, emission: new Color(0.2f, 0.9f, 0.1f) * 1.2f, tiling: new Vector2(6, 6), emissionMap: acidTex);
        var waterTex = Tex("water", 256, (x, y) => { float n = Fbm(x * 0.05f, y * 0.05f, 3); return Color.Lerp(new Color(0.02f, 0.1f, 0.2f), new Color(0.3f, 0.6f, 0.9f), n); });
        waterMat = Mat("Water", Color.white, waterTex, 0.95f, emission: new Color(0.1f, 0.3f, 0.5f), emissionMap: waterTex);
        goldMat = Mat("GlowGold", new Color(1f, 0.75f, 0.2f), null, 0.8f, metallic: 1f, emission: new Color(1f, 0.6f, 0.1f) * 2.5f);
        silverMat = Mat("GlowSilver", new Color(0.8f, 0.85f, 1f), null, 0.8f, metallic: 1f, emission: new Color(0.5f, 0.7f, 1f) * 2.5f);
        blackMat = Mat("Void", Color.black, null, 0f);
        var crackTex = Tex("cracked", 256, (x, y) => { float n = Fbm(x * 0.02f, y * 0.02f, 4); float crack = Mathf.Abs(Fbm(x * 0.015f + 3, y * 0.015f + 9, 2) - 0.5f) < 0.025f || Mathf.Abs(Fbm(x * 0.03f + 11, y * 0.03f, 2) - 0.5f) < 0.015f ? 0.1f : 1f; float v = Mathf.Lerp(0.2f, 0.36f, n) * crack; return new Color(v * 1.1f, v, v * 0.85f); });
        crackMat = Mat("CrackedFloor", Color.white, crackTex, 0.1f);
        var dot = Tex("softdot", 64, (x, y) => { float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f)) / 32f; float a = Mathf.Clamp01(1 - d); a *= a; return new Color(1, 1, 1, a); }, alpha: true);
        particleMat = ParticleMat("SoftParticle", dot);
        mapTex = MakeMapTexture();
        parchMat = Mat("TreasureMap", Color.white, mapTex, 0.05f);

        rockMeshes = new Mesh[6];
        for (int i = 0; i < rockMeshes.Length; i++) rockMeshes[i] = SaveMesh(RockMesh(100 + i * 17, 0.35f), "rock" + i);
        stalactiteMesh = SaveMesh(StalactiteMesh(), "stalactite");
        halfDiscMesh = SaveMesh(HalfDisc(24), "halfdisc");

        InitKay();
        volumeProfile = MakeVolumeProfile();
        playerAnim = BuildAnimator();
    }

    // ------------------------------------------------------------------ noise & textures
    static float Fbm(float x, float y, int oct)
    {
        float s = 0, a = 0.5f, f = 1, norm = 0;
        for (int i = 0; i < oct; i++) { s += a * Mathf.PerlinNoise(x * f + i * 13.1f, y * f + i * 7.7f); norm += a; a *= 0.5f; f *= 2f; }
        return s / norm;
    }

    static Texture2D Tex(string name, int size, Func<int, int, Color> f, bool alpha = false)
    {
        string path = $"{Gen}/Textures/{name}.png";
        var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px = new Color[size * size];
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) px[y * size + x] = f(x, y);
        t.SetPixels(px); t.Apply();
        return SaveTex(t, path, alpha);
    }

    static Texture2D SaveTex(Texture2D t, string path, bool alpha)
    {
        File.WriteAllBytes(path, t.EncodeToPNG());
        Object.DestroyImmediate(t);
        AssetDatabase.ImportAsset(path);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.wrapMode = TextureWrapMode.Repeat; imp.alphaIsTransparency = alpha; imp.mipmapEnabled = true;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static readonly Dictionary<string, Material> matCache = new Dictionary<string, Material>();

    static Material Mat(string name, Color c, Texture tex, float smooth, float metallic = 0, Color? emission = null, Vector2? tiling = null, Texture emissionMap = null)
    {
        string path = $"{Gen}/Materials/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
        m.shader = Shader.Find("Universal Render Pipeline/Lit");
        m.SetColor("_BaseColor", c);
        m.SetTexture("_BaseMap", tex);
        m.SetTextureScale("_BaseMap", tiling ?? Vector2.one);
        m.SetFloat("_Smoothness", smooth);
        m.SetFloat("_Metallic", metallic);
        if (emission.HasValue)
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission.Value);
            if (emissionMap) m.SetTexture("_EmissionMap", emissionMap);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else { m.DisableKeyword("_EMISSION"); m.SetColor("_EmissionColor", Color.black); }
        EditorUtility.SetDirty(m);
        matCache[name] = m;
        return m;
    }

    static Material Emissive(string name, Color c, float intensity = 2f) => Mat(name, c, null, 0.4f, 0, c * intensity);

    static Material ParticleMat(string name, Texture tex)
    {
        string path = $"{Gen}/Materials/{name}.mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        var sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (!m) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); }
        m.shader = sh;
        m.SetTexture("_BaseMap", tex);
        m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 2); // additive
        m.SetOverrideTag("RenderType", "Transparent");
        m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha); m.SetInt("_DstBlend", (int)BlendMode.One); m.SetInt("_ZWrite", 0);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); m.EnableKeyword("_BLENDMODE_ADD");
        m.renderQueue = 3000;
        EditorUtility.SetDirty(m);
        return m;
    }

    static Texture2D MakeMapTexture()
    {
        int W = 1024, H = 1024;
        var px = new Color[W * H];
        for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                float n = Fbm(x * 0.006f, y * 0.006f, 5);
                float edge = Mathf.Min(Mathf.Min(x, W - x), Mathf.Min(y, H - y)) / 60f + (Fbm(x * 0.03f, y * 0.03f, 2) - 0.5f) * 0.8f;
                var c = Color.Lerp(new Color(0.72f, 0.58f, 0.38f), new Color(0.92f, 0.82f, 0.62f), n);
                c = Color.Lerp(new Color(0.25f, 0.15f, 0.07f), c, Mathf.Clamp01(edge));
                px[y * W + x] = c;
            }
        Color ink = new Color(0.25f, 0.13f, 0.06f), red = new Color(0.65f, 0.05f, 0.03f);
        void P(int x, int y, Color c, int r = 2) { for (int dy = -r; dy <= r; dy++) for (int dx = -r; dx <= r; dx++) { int xx = x + dx, yy = y + dy; if (xx >= 0 && yy >= 0 && xx < W && yy < H && dx * dx + dy * dy <= r * r) px[yy * W + xx] = c; } }
        void L(Vector2 a, Vector2 b, Color c, int r = 2, int dash = 0) { int n = (int)Vector2.Distance(a, b); for (int i = 0; i <= n; i++) { if (dash > 0 && (i / dash) % 2 == 1) continue; var p = Vector2.Lerp(a, b, i / (float)Mathf.Max(1, n)); P((int)p.x, (int)p.y, c, r); } }
        void Circ(Vector2 c0, float rad, Color c, int r = 2) { for (int i = 0; i < 180; i++) { float a = i / 180f * Mathf.PI * 2; P((int)(c0.x + Mathf.Cos(a) * rad), (int)(c0.y + Mathf.Sin(a) * rad), c, r); } }
        // mountains
        for (int i = 0; i < 7; i++) { float x = 520 + i * 60, y = 640 + (i % 2) * 30; L(new Vector2(x - 45, y), new Vector2(x, y + 90), ink, 3); L(new Vector2(x, y + 90), new Vector2(x + 45, y), ink, 3); }
        // river
        Vector2 prev = new Vector2(80, 900);
        for (int i = 1; i < 30; i++) { var p = new Vector2(80 + i * 14, 900 - i * 10 + Mathf.Sin(i * 0.8f) * 25); L(prev, p, new Color(0.2f, 0.3f, 0.5f), 3); prev = p; }
        // study (house) bottom-left
        L(new Vector2(120, 140), new Vector2(220, 140), ink, 3); L(new Vector2(120, 140), new Vector2(120, 220), ink, 3); L(new Vector2(220, 140), new Vector2(220, 220), ink, 3); L(new Vector2(110, 220), new Vector2(170, 280), ink, 3); L(new Vector2(170, 280), new Vector2(230, 220), ink, 3);
        // dotted trail to cave
        Vector2[] trail = { new Vector2(230, 180), new Vector2(360, 260), new Vector2(420, 400), new Vector2(560, 470), new Vector2(690, 600) };
        for (int i = 0; i < trail.Length - 1; i++) L(trail[i], trail[i + 1], red, 3, 10);
        // cave arch
        for (int i = 0; i <= 40; i++) { float a = Mathf.PI * i / 40f; P((int)(700 + Mathf.Cos(a) * 55), (int)(600 + Mathf.Sin(a) * 60), ink, 5); }
        // underground shaft & levels
        L(new Vector2(700, 600), new Vector2(700, 440), ink, 2, 8);
        Circ(new Vector2(700, 400), 34, ink, 3); for (int i = 0; i < 6; i++) { float a = i * Mathf.PI / 3; L(new Vector2(700, 400), new Vector2(700 + Mathf.Cos(a) * 34, 400 + Mathf.Sin(a) * 34), ink, 2); } // fan tower
        for (int i = 0; i < 4; i++) { L(new Vector2(780 + i * 25, 300), new Vector2(780 + i * 25, 380), ink, 2); L(new Vector2(780, 300 + i * 25), new Vector2(855, 300 + i * 25), ink, 2); } // maze
        L(new Vector2(735, 400), new Vector2(780, 340), red, 3, 8);
        // X marks the spot
        L(new Vector2(860, 180), new Vector2(920, 240), red, 6); L(new Vector2(860, 240), new Vector2(920, 180), red, 6);
        L(new Vector2(830, 300), new Vector2(890, 245), red, 3, 8);
        // sun & moon keys emblem
        Circ(new Vector2(160, 520), 40, new Color(0.7f, 0.45f, 0.05f), 4); for (int i = 0; i < 12; i++) { float a = i * Mathf.PI / 6; L(new Vector2(160 + Mathf.Cos(a) * 50, 520 + Mathf.Sin(a) * 50), new Vector2(160 + Mathf.Cos(a) * 70, 520 + Mathf.Sin(a) * 70), new Color(0.7f, 0.45f, 0.05f), 3); }
        Circ(new Vector2(300, 520), 40, new Color(0.3f, 0.35f, 0.5f), 4); Circ(new Vector2(318, 530), 34, new Color(0.3f, 0.35f, 0.5f), 2);
        // compass
        L(new Vector2(900, 900), new Vector2(900, 1000), ink, 3); L(new Vector2(850, 950), new Vector2(950, 950), ink, 3); L(new Vector2(885, 985), new Vector2(900, 1005), ink, 3); L(new Vector2(915, 985), new Vector2(900, 1005), ink, 3);
        var t = new Texture2D(W, H, TextureFormat.RGBA32, false); t.SetPixels(px); t.Apply();
        return SaveTex(t, $"{Gen}/Textures/treasure_map.png", false);
    }

    // ------------------------------------------------------------------ meshes
    static Mesh SaveMesh(Mesh m, string name)
    {
        string path = $"{Gen}/Meshes/{name}.asset";
        var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (old) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static float Noise3(Vector3 p) => (Mathf.PerlinNoise(p.x, p.y) + Mathf.PerlinNoise(p.y + 3.1f, p.z) + Mathf.PerlinNoise(p.z + 7.3f, p.x)) / 3f;

    static Mesh RockMesh(int seed, float amp)
    {
        int lat = 12, lon = 18; var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>();
        Vector3 off = new Vector3(seed * 0.37f, seed * 0.11f, seed * 0.73f);
        Vector3 stretch = new Vector3(1f + (seed % 3) * 0.2f, 0.7f + (seed % 5) * 0.08f, 1f);
        for (int i = 0; i <= lat; i++)
        {
            float th = Mathf.PI * i / lat;
            for (int j = 0; j <= lon; j++)
            {
                float ph = 2 * Mathf.PI * j / lon;
                var d = new Vector3(Mathf.Sin(th) * Mathf.Cos(ph), Mathf.Cos(th), Mathf.Sin(th) * Mathf.Sin(ph));
                float n = Noise3(d * 1.6f + off) * 0.7f + Noise3(d * 4f + off) * 0.3f;
                var p = d * (1f + (n - 0.5f) * 2f * amp);
                v.Add(Vector3.Scale(p, stretch) * 0.5f); uv.Add(new Vector2(j / (float)lon * 2, i / (float)lat));
            }
        }
        for (int i = 0; i < lat; i++) for (int j = 0; j < lon; j++)
            {
                int a = i * (lon + 1) + j, b = a + lon + 1;
                tri.Add(a); tri.Add(a + 1); tri.Add(b); tri.Add(a + 1); tri.Add(b + 1); tri.Add(b);
            }
        var m = new Mesh(); m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0); m.RecalculateNormals(); m.RecalculateBounds(); m.name = "rock" + seed;
        return m;
    }

    static Mesh StalactiteMesh()
    {
        int seg = 8; var v = new List<Vector3>(); var tri = new List<int>(); var uv = new List<Vector2>();
        for (int j = 0; j <= seg; j++) { float a = j * Mathf.PI * 2 / seg; v.Add(new Vector3(Mathf.Cos(a) * 0.5f, 0, Mathf.Sin(a) * 0.5f)); uv.Add(new Vector2(j / (float)seg, 1)); }
        v.Add(new Vector3(0, -1, 0)); uv.Add(new Vector2(0.5f, 0));
        int tip = v.Count - 1;
        for (int j = 0; j < seg; j++) { tri.Add(j); tri.Add(j + 1); tri.Add(tip); }
        var m = new Mesh(); m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0); m.RecalculateNormals(); m.name = "stalactite"; return m;
    }

    static Mesh HalfDisc(int seg)
    {
        var v = new List<Vector3> { Vector3.zero }; var tri = new List<int>();
        for (int i = 0; i <= seg; i++) { float a = Mathf.PI * i / seg; v.Add(new Vector3(Mathf.Cos(a) * 0.5f, Mathf.Sin(a) * 0.5f, 0)); }
        for (int i = 1; i <= seg; i++) { tri.Add(0); tri.Add(i + 1); tri.Add(i); }
        var m = new Mesh(); m.SetVertices(v); m.SetUVs(0, v.Select(p => new Vector2(p.x + 0.5f, p.y)).ToList()); m.SetTriangles(tri, 0); m.RecalculateNormals(); m.name = "halfdisc"; return m;
    }

    /// Cave tunnel along a list of floor points: noisy elliptical rings with a flat floor, double-sided.
    static Mesh TunnelMesh(List<Vector3> floor, Func<int, float> halfW, Func<int, float> halfH, float floorLift, int seg, float noise, Func<int, bool> holeAt, out List<float> floorHalfWidth)
    {
        var v = new List<Vector3>(); var uv = new List<Vector2>(); var tri = new List<int>(); var clamped = new List<bool>();
        floorHalfWidth = new List<float>();
        float along = 0;
        for (int i = 0; i < floor.Count; i++)
        {
            Vector3 t = (floor[Mathf.Min(i + 1, floor.Count - 1)] - floor[Mathf.Max(i - 1, 0)]); t.y = 0; t.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, t).normalized;
            Vector3 c = floor[i] + Vector3.up * floorLift;
            if (i > 0) along += Vector3.Distance(floor[i], floor[i - 1]);
            float w = halfW(i), h = halfH(i);
            float fw = 0;
            for (int j = 0; j <= seg; j++)
            {
                float a = 2 * Mathf.PI * j / seg - Mathf.PI / 2; // start at the bottom
                float x = Mathf.Cos(a) * w, y = Mathf.Sin(a) * h;
                bool cl = y < -floorLift;
                if (cl) { y = -floorLift; fw = Mathf.Max(fw, Mathf.Abs(x)); }
                Vector3 p = c + right * x + Vector3.up * y;
                float n = Noise3(p * 0.35f) - 0.5f;
                Vector3 radial = (right * Mathf.Cos(a) + Vector3.up * Mathf.Sin(a)).normalized;
                p += cl ? Vector3.up * n * 0.06f : radial * n * noise * 2f;
                v.Add(p); uv.Add(new Vector2(j / (float)seg * 5f, along / 4f)); clamped.Add(cl);
            }
            floorHalfWidth.Add(fw);
        }
        int R = seg + 1;
        for (int i = 0; i < floor.Count - 1; i++)
            for (int j = 0; j < seg; j++)
            {
                int a = i * R + j, b = (i + 1) * R + j, c2 = a + 1, d = b + 1;
                if (holeAt(i) && clamped[a] && clamped[c2] && clamped[b] && clamped[d]) continue;
                // winding chosen so the normal points into the tunnel
                tri.Add(a); tri.Add(c2); tri.Add(b); tri.Add(c2); tri.Add(d); tri.Add(b);
            }
        return DoubleSided(v, uv, tri, "tunnel");
    }

    static Mesh DoubleSided(List<Vector3> v, List<Vector2> uv, List<int> tri, string name)
    {
        int n = v.Count;
        var v2 = new List<Vector3>(v); v2.AddRange(v);
        var uv2 = new List<Vector2>(uv); uv2.AddRange(uv);
        var t2 = new List<int>(tri);
        for (int i = 0; i < tri.Count; i += 3) { t2.Add(tri[i] + n); t2.Add(tri[i + 2] + n); t2.Add(tri[i + 1] + n); }
        var m = new Mesh { indexFormat = IndexFormat.UInt32, name = name };
        m.SetVertices(v2); m.SetUVs(0, uv2); m.SetTriangles(t2, 0); m.RecalculateNormals(); m.RecalculateBounds();
        return m;
    }

    static Mesh DiscMesh(float radius, int seg, float uvScale)
    {
        var v = new List<Vector3> { Vector3.zero }; var uv = new List<Vector2> { Vector2.zero }; var tri = new List<int>();
        for (int i = 0; i <= seg; i++) { float a = 2 * Mathf.PI * i / seg; var p = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius); v.Add(p); uv.Add(new Vector2(p.x, p.z) / uvScale); }
        for (int i = 1; i <= seg; i++) { tri.Add(0); tri.Add(i + 1); tri.Add(i); }
        var m = new Mesh(); m.SetVertices(v); m.SetUVs(0, uv); m.SetTriangles(tri, 0); m.RecalculateNormals(); m.RecalculateBounds(); return m;
    }

    // ------------------------------------------------------------------ KayKit placement
    static readonly Dictionary<string, bool> nativeAlongX = new Dictionary<string, bool>();

    static GameObject KayPrefab(string n) => AssetDatabase.LoadAssetAtPath<GameObject>($"{KayDir}/{n}.fbx");

    static Bounds RendBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b;
    }

    static void InitKay()
    {
        var w = KayPrefab("wall");
        if (!w) { Debug.LogWarning("[ADV] KayKit wall not found"); return; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(w);
        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); go.transform.localScale = Vector3.one;
        var b = RendBounds(go); Object.DestroyImmediate(go);
        float width = Mathf.Max(b.size.x, b.size.z);
        kScale = width > 0.001f ? 4f / width : 1f;
        wallH = b.size.y * kScale;
        Debug.Log($"[ADV] KayKit wall size {b.size} -> scale {kScale}, wallH {wallH}");
    }

    static bool AlongX(string name)
    {
        if (nativeAlongX.TryGetValue(name, out var ax)) return ax;
        var p = KayPrefab(name); if (!p) return true;
        var go = (GameObject)PrefabUtility.InstantiatePrefab(p);
        go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        var b = RendBounds(go); Object.DestroyImmediate(go);
        ax = b.size.x >= b.size.z; nativeAlongX[name] = ax; return ax;
    }

    static float YawToward(string name, Vector3 dir)
    {
        return AlongX(name) ? Mathf.Atan2(-dir.z, dir.x) * Mathf.Rad2Deg : Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
    }

    /// Places a KayKit piece with its bounds' bottom-centre at pos.
    static GameObject Kay(string name, Vector3 pos, float yaw, Transform parent, bool collider = true, float scaleMul = 1f, float fitXZ = 0f, Material mat = null)
    {
        var p = KayPrefab(name);
        if (!p) { Debug.LogWarning("[ADV] missing KayKit piece " + name); return null; }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(p, parent);
        go.transform.rotation = Quaternion.Euler(0, yaw, 0);
        go.transform.localScale = Vector3.one * kScale * scaleMul;
        go.transform.position = pos;
        if (fitXZ > 0)
        {
            var b0 = RendBounds(go); float m = Mathf.Max(b0.size.x, b0.size.z);
            if (m > 0.001f) go.transform.localScale *= fitXZ / m;
        }
        var b = RendBounds(go);
        go.transform.position += new Vector3(pos.x - b.center.x, pos.y - b.min.y, pos.z - b.center.z);
        foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterials = Enumerable.Repeat(mat ? mat : kayMat, r.sharedMaterials.Length).ToArray();
        if (collider) foreach (var mf in go.GetComponentsInChildren<MeshFilter>()) mf.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
        return go;
    }

    static GameObject KayWall(string name, Vector3 edgeCentre, Vector3 dir, Transform parent) => Kay(name, edgeCentre, YawToward(name, dir), parent);

    // ------------------------------------------------------------------ primitives & lights
    static GameObject Prim(PrimitiveType t, Vector3 pos, Vector3 scale, Quaternion rot, Material m, Transform parent, bool collider = true, string name = null)
    {
        var go = GameObject.CreatePrimitive(t);
        if (name != null) go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(pos, rot); go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = m;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static GameObject MeshObj(string name, Mesh mesh, Material m, Vector3 pos, Quaternion rot, Vector3 scale, Transform parent, bool collider = true, bool convex = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(pos, rot); go.transform.localScale = scale;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = m;
        if (collider) { var mc = go.AddComponent<MeshCollider>(); mc.sharedMesh = mesh; mc.convex = convex; }
        return go;
    }

    static GameObject Rock(Vector3 pos, float size, Transform parent, bool collider = true, Material m = null)
    {
        var mesh = rockMeshes[Random.Range(0, rockMeshes.Length)];
        var s = new Vector3(Random.Range(0.8f, 1.3f), Random.Range(0.6f, 1.1f), Random.Range(0.8f, 1.3f)) * size;
        return MeshObj("Rock", mesh, m ? m : rockMat, pos, Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0, 360f), Random.Range(-15f, 15f)), s, parent, collider);
    }

    static Light PointLight(Vector3 pos, Color c, float intensity, float range, Transform parent, bool flicker = false, bool shadows = false)
    {
        var go = new GameObject("Light"); go.transform.SetParent(parent, false); go.transform.position = pos;
        var l = go.AddComponent<Light>(); l.type = LightType.Point; l.color = c; l.intensity = intensity; l.range = range;
        l.shadows = shadows ? LightShadows.Soft : LightShadows.None;
        if (flicker) go.AddComponent<FlickerLight>();
        return l;
    }

    static ParticleSystem Particles(string name, Vector3 pos, Transform parent, Color c, float rate, float life, float speed, float size, Vector3 box, Vector3 gravityDir, float gravity = 0)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main; main.startLifetime = life; main.startSpeed = speed; main.startSize = size; main.startColor = c; main.maxParticles = 500; main.gravityModifier = gravity; main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission; em.rateOverTime = rate;
        var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = box;
        go.transform.rotation = Quaternion.LookRotation(gravityDir == Vector3.zero ? Vector3.up : gravityDir);
        var col = ps.colorOverLifetime; col.enabled = true;
        var g = new Gradient(); g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) }, new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.2f), new GradientAlphaKey(0, 1) });
        col.color = g;
        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = particleMat;
        return ps;
    }

    static void Torch(Vector3 pos, Transform parent, bool mounted, Vector3 facing)
    {
        var t = Kay(mounted ? "torch_mounted" : "torch_lit", pos, facing == Vector3.zero ? 0 : Mathf.Atan2(facing.x, facing.z) * Mathf.Rad2Deg, parent, false);
        float top = t ? RendBounds(t).max.y : pos.y + 1;
        var flameAt = new Vector3(pos.x, top + 0.1f, pos.z) + facing * 0.15f;
        PointLight(flameAt + facing * 0.3f, new Color(1f, 0.6f, 0.25f), 6f, 11f, parent, true);
        var ps = Particles("Flame", flameAt, parent, new Color(1f, 0.55f, 0.15f), 25, 0.6f, 0.6f, 0.25f, new Vector3(0.1f, 0.1f, 0.1f), Vector3.up);
        var sz = ps.sizeOverLifetime; sz.enabled = true; sz.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 0.1f));
    }

    // ------------------------------------------------------------------ scene scaffolding
    static Transform NewScene(string name, Color ambient, Color fogColor, float fogDensity)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambient;
        RenderSettings.fog = fogDensity > 0; RenderSettings.fogMode = FogMode.ExponentialSquared; RenderSettings.fogColor = fogColor; RenderSettings.fogDensity = fogDensity;
        RenderSettings.skybox = null;
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        var root = new GameObject(name).transform;
        var vol = new GameObject("Global Volume").AddComponent<Volume>(); vol.isGlobal = true; vol.sharedProfile = volumeProfile;
        return root;
    }

    static void SaveScene(string name)
    {
        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(scene, $"{ScenesDir}/{name}.unity");
    }

    static VolumeProfile MakeVolumeProfile()
    {
        string path = $"{Gen}/PostFX.asset";
        if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(path)) AssetDatabase.DeleteAsset(path);
        var p = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(p, path);
        var bloom = p.Add<Bloom>(true); bloom.intensity.Override(1.1f); bloom.threshold.Override(0.85f); bloom.scatter.Override(0.65f);
        var vig = p.Add<Vignette>(true); vig.intensity.Override(0.38f); vig.smoothness.Override(0.5f);
        var tm = p.Add<Tonemapping>(true); tm.mode.Override(TonemappingMode.ACES);
        var ca = p.Add<ColorAdjustments>(true); ca.postExposure.Override(0.35f); ca.contrast.Override(12f); ca.saturation.Override(-5f);
        foreach (var c in p.components) AssetDatabase.AddObjectToAsset(c, p);
        EditorUtility.SetDirty(p);
        return p;
    }

    static RuntimeAnimatorController BuildAnimator()
    {
        var imp = AssetImporter.GetAtPath(CharFbx) as ModelImporter;
        if (imp == null) { Debug.LogWarning("[ADV] character fbx missing"); return null; }
        string[] loops = { "Idle", "Walking_A", "Running_A", "Jump_Idle" };
        var clips = imp.clipAnimations; if (clips == null || clips.Length == 0) clips = imp.defaultClipAnimations;
        bool changed = imp.animationType != ModelImporterAnimationType.Generic || imp.clipAnimations.Length == 0;
        foreach (var c in clips) { bool l = loops.Contains(c.name); if (c.loopTime != l) { c.loopTime = l; changed = true; } }
        if (changed) { imp.animationType = ModelImporterAnimationType.Generic; imp.clipAnimations = clips; imp.SaveAndReimport(); }
        var all = AssetDatabase.LoadAllAssetsAtPath(CharFbx).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToList();
        AnimationClip Find(params string[] names) { foreach (var n in names) { var c = all.FirstOrDefault(x => x.name == n) ?? all.FirstOrDefault(x => x.name.ToLower().Contains(n.ToLower())); if (c) return c; } return all.FirstOrDefault(); }
        var idle = Find("Idle"); var walk = Find("Walking_A", "Walk"); var run = Find("Running_A", "Run"); var air = Find("Jump_Idle", "Jump");
        string path = $"{Gen}/PlayerAnimator.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path)) AssetDatabase.DeleteAsset(path);
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        var ps = ctrl.parameters; ps[1].defaultBool = true; ctrl.parameters = ps;
        var sm = ctrl.layers[0].stateMachine;
        var loco = ctrl.CreateBlendTreeInController("Locomotion", out BlendTree tree, 0);
        tree.blendParameter = "Speed"; tree.useAutomaticThresholds = false;
        tree.AddChild(idle, 0f); tree.AddChild(walk, 2.2f); tree.AddChild(run, 5.5f);
        var airState = sm.AddState("Air"); airState.motion = air;
        var t1 = loco.AddTransition(airState); t1.hasExitTime = false; t1.duration = 0.1f; t1.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
        var t2 = airState.AddTransition(loco); t2.hasExitTime = false; t2.duration = 0.1f; t2.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        sm.defaultState = loco;
        EditorUtility.SetDirty(ctrl);
        Debug.Log($"[ADV] Animator clips: idle={idle?.name} walk={walk?.name} run={run?.name} air={air?.name} (total {all.Count})");
        return ctrl;
    }

    static readonly string[] hideWords = { "knife", "crossbow", "dagger", "throwable", "bow", "sword", "shield", "axe", "mug", "staff", "wand", "spellbook", "quiver", "arrow" };

    static PlayerController CreatePlayer(Vector3 pos, float yaw)
    {
        var player = new GameObject("Player"); player.layer = 2;
        player.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
        var cc = player.AddComponent<CharacterController>(); cc.height = 1.8f; cc.radius = 0.35f; cc.center = new Vector3(0, 0.92f, 0); cc.stepOffset = 0.4f; cc.slopeLimit = 50f; cc.skinWidth = 0.04f;
        var pc = player.AddComponent<PlayerController>();
        player.AddComponent<PlayerInteractor>();
        Transform hand = null;
        var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(CharFbx);
        if (fbx)
        {
            var model = (GameObject)PrefabUtility.InstantiatePrefab(fbx, player.transform);
            model.name = "Archaeologist";
            model.transform.localPosition = Vector3.zero; model.transform.localRotation = Quaternion.identity; model.transform.localScale = Vector3.one;
            foreach (var r in model.GetComponentsInChildren<Renderer>(true))
            {
                if (hideWords.Any(w => r.name.ToLower().Contains(w))) { r.gameObject.SetActive(false); continue; }
                r.sharedMaterials = Enumerable.Repeat(charMat, r.sharedMaterials.Length).ToArray();
            }
            var b = RendBounds(model);
            float s = 1.8f / Mathf.Max(0.01f, b.size.y);
            model.transform.localScale = Vector3.one * s;
            b = RendBounds(model);
            model.transform.position += Vector3.up * (pos.y - b.min.y);
            foreach (var t in model.GetComponentsInChildren<Transform>(true)) { t.gameObject.layer = 2; if (t.name.ToLower() == "handslot.r") hand = t; }
            var anim = model.GetComponent<Animator>(); if (anim == null) anim = model.AddComponent<Animator>();
            anim.runtimeAnimatorController = playerAnim; anim.applyRootMotion = false; anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            pc.animator = anim;
            if (hand)
            {
                var torchBody = Prim(PrimitiveType.Cylinder, hand.position, new Vector3(0.09f, 0.16f, 0.09f), hand.rotation, brassMat, hand, false, "TorchBody");
                torchBody.transform.localScale = new Vector3(0.09f, 0.16f, 0.09f) / s;
                torchBody.layer = 2;
            }
        }
        else
        {
            var cap = Prim(PrimitiveType.Capsule, pos + Vector3.up * 0.9f, Vector3.one, Quaternion.identity, charMat, player.transform, false); cap.layer = 2;
        }
        // torch light
        var torch = player.AddComponent<TorchLight>();
        var spotGo = new GameObject("TorchSpot"); spotGo.transform.SetParent(player.transform, false); spotGo.transform.localPosition = new Vector3(0.25f, 1.4f, 0.3f);
        var spot = spotGo.AddComponent<Light>(); spot.type = LightType.Spot; spot.range = 45f; spot.spotAngle = 70f; spot.innerSpotAngle = 30f; spot.intensity = 45f; spot.color = new Color(1f, 0.92f, 0.78f); spot.shadows = LightShadows.Soft;
        var fillGo = new GameObject("TorchFill"); fillGo.transform.SetParent(player.transform, false); fillGo.transform.localPosition = new Vector3(0, 1.6f, 0.4f);
        var fill = fillGo.AddComponent<Light>(); fill.type = LightType.Point; fill.range = 9f; fill.intensity = 3f; fill.color = new Color(1f, 0.85f, 0.7f);
        torch.spot = spot; torch.fill = fill; torch.hand = hand;

        // camera
        var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>(); cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black; cam.nearClipPlane = 0.1f; cam.farClipPlane = 300f;
        camGo.AddComponent<AudioListener>();
        var data = camGo.AddComponent<UniversalAdditionalCameraData>(); data.renderPostProcessing = true;
        var tpc = camGo.AddComponent<ThirdPersonCamera>(); tpc.target = player.transform;
        camGo.transform.position = pos + Quaternion.Euler(12, yaw, 0) * Vector3.back * 4.5f + Vector3.up * 1.6f;
        camGo.transform.LookAt(pos + Vector3.up * 1.6f);
        return pc;
    }

    static LevelIntro Intro(string levelName, string objective, string next, params StoryPage[] pages)
    {
        var li = new GameObject("LevelIntro").AddComponent<LevelIntro>();
        li.levelName = levelName; li.objective = objective; li.nextScene = next; li.pages = pages.ToList();
        li.mapTexture = mapTex; li.mapLegend = MapLegend;
        return li;
    }

    static T Zone<T>(string name, Vector3 centre, Vector3 size, Transform parent, float yaw = 0) where T : PlayerZone
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(centre, Quaternion.Euler(0, yaw, 0));
        var bc = go.AddComponent<BoxCollider>(); bc.isTrigger = true; bc.size = size;
        return go.AddComponent<T>();
    }

    const string MapLegend =
        "THE MAP OF THE OLD KINGS\n\n" +
        "• The house — your study, where the journey begins.\n" +
        "• Red trail — the road to the Old Cave in the mountains.\n" +
        "• Beneath the cave — a great shaft into the deep.\n" +
        "• The wheel — the Tower of Turning Fans. Cross it to the top.\n" +
        "• The grid — the Labyrinth. The proverb is the only guide.\n" +
        "• The red X — the treasure vault.\n\n" +
        "Margin note: \"Sun and Moon together open the way.\"\n\n" +
        "Press M any time to view this map.  (Enter to close)";
}
