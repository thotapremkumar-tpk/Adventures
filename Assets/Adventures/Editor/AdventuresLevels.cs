using System.Collections.Generic;
using System.Linq;
using Adventures;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public static partial class AdventuresBuilder
{
    static StoryPage Pg(string t, string b) => new StoryPage(t, b);

    // =================================================================== INTRO: the study
    static void BuildIntro()
    {
        var root = NewScene("Study", new Color(0.3f, 0.25f, 0.2f), Color.black, 0f);
        // floor 3x3 cells, walls around, doorway north
        for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z++)
            {
                Kay("floor_tile_large", new Vector3(x * 4, 0, z * 4), 0, root, false, 1, 4f);
            }
        var floorCol = Prim(PrimitiveType.Cube, new Vector3(0, -0.25f, 0), new Vector3(12, 0.5f, 12), Quaternion.identity, stoneMat, root, true, "FloorCollider");
        floorCol.GetComponent<Renderer>().enabled = false;
        for (int i = -1; i <= 1; i++)
        {
            var nw = KayWall(i == 0 ? "wall_doorway" : "wall", new Vector3(i * 4, 0, 6), Vector3.right, root);
            if (i == 0 && nw)
            {
                var doorPart = nw.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name.ToLower().Contains("door") && t.name.ToLower() != "wall_doorway" && t.GetComponent<Renderer>());
                if (doorPart)
                {
                    var sd = doorPart.gameObject.AddComponent<SwingDoor>();
                    sd.prompt = "Open the door"; sd.range = 3.5f; sd.requiredItem = "map";
                    sd.lockedMessage = "I should study that old map on my desk before I leave.";
                }
                else Debug.LogWarning("[ADV] doorway door mesh not found");
            }
            KayWall(i == 0 ? "wall_arched" : "wall", new Vector3(i * 4, 0, -6), Vector3.right, root);
            KayWall(i == 0 ? "wall_shelves" : "wall", new Vector3(-6, 0, i * 4), Vector3.forward, root);
            KayWall("wall", new Vector3(6, 0, i * 4), Vector3.forward, root);
        }
        foreach (var c in new[] { new Vector3(-6, 0, -6), new Vector3(6, 0, -6), new Vector3(-6, 0, 6), new Vector3(6, 0, 6) }) Kay("pillar", c, 0, root);
        Prim(PrimitiveType.Cube, new Vector3(0, wallH + 0.1f, 0), new Vector3(13, 0.2f, 13), Quaternion.identity, rockDarkMat, root, false, "Ceiling");

        // desk with the map + Sun Key
        var table = Kay("table_medium_decorated_A", new Vector3(0, 0, 1), 0, root);
        float top = table ? RendBounds(table).max.y : 1f;
        Kay("candle_triple", new Vector3(-1.1f, top, 1.3f), 0, root, false);
        PointLight(new Vector3(-1.1f, top + 0.6f, 1.3f), new Color(1f, 0.7f, 0.4f), 1.6f, 7f, root, true);
        var mapGo = new GameObject("TreasureMap"); mapGo.transform.SetParent(root, false); mapGo.transform.position = new Vector3(0.2f, top + 0.02f, 0.9f);
        Prim(PrimitiveType.Cube, mapGo.transform.position, new Vector3(0.9f, 0.01f, 0.9f), Quaternion.Euler(0, 12, 0), parchMat, mapGo.transform, false, "Parchment");
        var key = Kay("key", new Vector3(0.95f, top + 0.02f, 0.8f), 30, mapGo.transform, false, 1f, 0.35f, goldMat);
        PointLight(mapGo.transform.position + Vector3.up * 0.8f, new Color(1f, 0.85f, 0.6f), 1.2f, 3f, mapGo.transform);
        var pick = mapGo.AddComponent<PickupItem>();
        pick.prompt = "Examine the old map"; pick.range = 2.6f;
        pick.itemIds = new[] { "map", "key_sun" };
        pick.toast = "Got: Old Map + Sun Key";
        pick.mapTexture = mapTex; pick.mapLegend = MapLegend;
        pick.newObjective = "Travel to the Old Cave — leave through the north door";
        pick.story = new List<StoryPage> {
            Pg("The Map of the Old Kings", "The parchment is older than any you have held. In faded ink the last Keeper of the Old Kings wrote:\n\n\"Our gold lies beneath the Old Cave, where the mountain swallows the light. Five trials guard it. Only one who carries the Sun and the Moon may reach it.\""),
            Pg("The Sun Key", "Wrapped inside the map is a heavy golden key shaped like a rising sun.\n\nOn its back is an inscription: \"I am half of the way. Find my sister, the Moon, in the house of turning winds.\"\n\nYou pack your big torch. It is time to go.")
        };
        // decoration
        Kay("shelf_small_candles", new Vector3(-5.2f, 0, 3.5f), 90, root);
        Kay("chest", new Vector3(4.6f, 0, -4.4f), -90, root);
        Kay("barrel_large", new Vector3(4.8f, 0, 4.6f), 0, root);
        Kay("crates_stacked", new Vector3(-4.6f, 0, -4.6f), 20, root);
        Kay("box_large", new Vector3(3.4f, 0, 4.8f), 10, root);
        Kay("banner_patternA_brown", new Vector3(5.7f, 1.2f, 0), -90, root, false);
        Kay("keyring", new Vector3(-5.6f, 1.6f, -1.5f), 90, root, false);
        PointLight(new Vector3(0, wallH - 0.6f, 0), new Color(1f, 0.78f, 0.55f), 2.2f, 14f, root, true, true);
        Torch(new Vector3(-5.6f, 1.8f, 0), root, true, Vector3.right);
        Torch(new Vector3(5.6f, 1.8f, -3), root, true, Vector3.left);
        // exit beyond the north door
        var exit = Zone<SceneExit>("ExitToCave", new Vector3(0, 1.2f, 7.5f), new Vector3(4, 3, 2), root);
        exit.sceneName = SCave; exit.requiredItem = "map"; exit.lockedMessage = "I should study that old map on my desk before I leave.";
        Prim(PrimitiveType.Cube, new Vector3(0, -0.25f, 8), new Vector3(4, 0.5f, 4), Quaternion.identity, rockDarkMat, root, true, "Porch");
        Prim(PrimitiveType.Cube, new Vector3(0, 2, 10.2f), new Vector3(5, 4, 0.2f), Quaternion.identity, blackMat, root, true, "Night");

        CreatePlayer(new Vector3(0, 0, -3.5f), 0);
        var li = Intro("TREASURE HUNTER", "Examine the old map on your desk (walk up and press E)", SCave,
            Pg("TREASURE HUNTER", "An Adventures demo — Levels 1 & 2\n\nFor twenty years you, an archaeologist, have searched for the lost Treasure of the Old Kings. Tonight a package arrived at your study with no name on it...\n\nControls: WASD move · Shift sprint · Space jump · Mouse look · E interact · F torch · H help"),
            Pg("A Mysterious Package", "Inside the package: an ancient map, and something heavy wrapped in its folds.\n\nTake a look at your desk."));
        li.ambientRumble = 0.15f; li.drips = false;
        SaveScene(SIntro);
    }

    // =================================================================== THE OLD CAVE
    static void BuildCave()
    {
        var root = NewScene("OldCave", new Color(0.07f, 0.07f, 0.08f), new Color(0.01f, 0.01f, 0.015f), 0.025f);
        int M = 66; const float step = 1.25f; int holeA = 54, holeB = 59;
        var floor = new List<Vector3>();
        for (int i = 0; i < M; i++) floor.Add(new Vector3(6f * Mathf.Sin(i * 0.07f), i < 6 ? 0 : -(i - 6) * 0.09f, i * step));
        System.Func<int, float> hw = i => i >= 48 ? 5.2f : 3.6f + 0.8f * Mathf.Sin(i * 0.11f + 1f);
        System.Func<int, float> hh = i => i >= 48 ? 4.5f : 3.1f + 0.9f * Mathf.Sin(i * 0.15f);
        var mesh = SaveMesh(TunnelMesh(floor, hw, hh, 1.7f, 28, 0.55f, i => i >= holeA && i < holeB, out var fw), "cave_tunnel");
        MeshObj("CaveTunnel", mesh, rockMat, Vector3.zero, Quaternion.identity, Vector3.one, root);

        // outside: moonlit ground + boulders around the mouth
        Prim(PrimitiveType.Plane, new Vector3(0, -0.02f, -18), new Vector3(6, 1, 4), Quaternion.identity, rockDarkMat, root, true, "Ground");
        for (int i = 0; i < 24; i++)
        {
            float side = i % 2 == 0 ? -1 : 1;
            Rock(new Vector3(side * Random.Range(6.5f, 16f), Random.Range(0f, 5f), Random.Range(-1f, 6f)), Random.Range(3f, 6f), root);
        }
        for (int i = 0; i < 8; i++) Rock(new Vector3(Random.Range(-3f, 3f), Random.Range(6.5f, 9f), Random.Range(1f, 8f)), Random.Range(3f, 5f), root);
        for (int i = 0; i < 10; i++) { float sx = i % 2 == 0 ? -1 : 1; Rock(new Vector3(sx * Random.Range(5f, 15f), 0.2f, Random.Range(-30f, -10f)), Random.Range(0.8f, 2.5f), root); }
        var moon = new GameObject("Moonlight").AddComponent<Light>(); moon.transform.SetParent(root, false);
        moon.type = LightType.Spot; moon.transform.position = new Vector3(0, 18, -14); moon.transform.rotation = Quaternion.Euler(70, 0, 0);
        moon.range = 40; moon.spotAngle = 90; moon.intensity = 12f; moon.color = new Color(0.55f, 0.65f, 1f);
        // abandoned camp at the entrance
        Kay("crates_stacked", new Vector3(-3.5f, 0, -3f), 15, root);
        Kay("barrel_small_stack", new Vector3(3.2f, 0, -2.5f), 0, root);
        Kay("box_small", new Vector3(2.4f, 0, -4f), 30, root);
        Torch(new Vector3(2.5f, 0, 0.5f), root, false, Vector3.zero);

        // detail inside the tunnel
        for (int i = 2; i < M - 1; i += 2)
        {
            Vector3 t = floor[Mathf.Min(i + 1, M - 1)] - floor[i - 1]; t.y = 0; t.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, t);
            if (i >= holeA - 1 && i <= holeB) continue;
            float w = Mathf.Max(fw[i], 2.5f);
            foreach (int s in new[] { -1, 1 }) if (Random.value < 0.8f) Rock(floor[i] + right * s * (w - 0.1f) + Vector3.up * 0.1f, Random.Range(0.6f, 1.5f), root);
            if (i % 3 == 0)
            {
                float ceil = floor[i].y + 1.7f + hh(i) * 0.92f;
                for (int k = 0; k < 2; k++)
                {
                    var p = floor[i] + right * Random.Range(-1.8f, 1.8f); p.y = ceil;
                    float len = Random.Range(0.7f, 1.8f);
                    MeshObj("Stalactite", stalactiteMesh, rockDarkMat, p, Quaternion.Euler(0, Random.value * 360, 0), new Vector3(Random.Range(0.3f, 0.6f), len, Random.Range(0.3f, 0.6f)), root, false);
                }
                if (i % 9 == 0) Particles("Drips", floor[i] + Vector3.up * (ceil - floor[i].y - 0.8f), root, new Color(0.6f, 0.75f, 1f, 0.8f), 2, 1.2f, 0, 0.06f, new Vector3(2, 0.1f, 2), Vector3.down, 1f);
            }
        }
        // old expedition debris along the way
        Kay("rubble_large", floor[14] + Vector3.right * 1.8f, 30, root);
        Kay("torch", floor[20] + Vector3.left * 1.5f, 80, root, false);
        Kay("rubble_half", floor[31] + Vector3.left * 2.0f, -20, root);
        Kay("box_small", floor[38] + Vector3.right * 2.2f, 45, root);
        Kay("sword_shield_broken", floor[44] + Vector3.left * 2.4f, 10, root, false);
        Particles("Dust", floor[25] + Vector3.up * 2, root, new Color(0.8f, 0.75f, 0.6f, 0.25f), 6, 6f, 0.1f, 0.05f, new Vector3(6, 3, 30), Vector3.up);

        // weak floor + shaft
        Vector3 hc = Vector3.zero; for (int i = holeA; i <= holeB; i++) hc += floor[i]; hc /= (holeB - holeA + 1);
        Vector3 ht = floor[holeB] - floor[holeA]; ht.y = 0; float yaw = Mathf.Atan2(ht.x, ht.z) * Mathf.Rad2Deg;
        float holeW = 2 * fw.Skip(holeA).Take(holeB - holeA).Max() + 0.8f, holeL = (holeB - holeA) * step + 0.8f;
        var slab = Prim(PrimitiveType.Cube, hc + Vector3.down * 0.25f, new Vector3(holeW, 0.5f, holeL), Quaternion.Euler(0, yaw, 0), crackMat, root, true, "WeakFloor");
        var shaftDepth = 26f;
        var shaftRoot = new GameObject("Shaft").transform; shaftRoot.SetParent(root, false); shaftRoot.SetPositionAndRotation(hc, Quaternion.Euler(0, yaw, 0));
        foreach (var s in new[] { -1, 1 })
        {
            var w1 = Prim(PrimitiveType.Cube, Vector3.zero, new Vector3(0.6f, shaftDepth, holeL + 1.2f), Quaternion.identity, rockMat, shaftRoot, true, "ShaftWall");
            w1.transform.localPosition = new Vector3(s * (holeW / 2 + 0.3f), -shaftDepth / 2 - 0.3f, 0); w1.transform.localRotation = Quaternion.identity;
            var w2 = Prim(PrimitiveType.Cube, Vector3.zero, new Vector3(holeW + 1.2f, shaftDepth, 0.6f), Quaternion.identity, rockMat, shaftRoot, true, "ShaftWall");
            w2.transform.localPosition = new Vector3(0, -shaftDepth / 2 - 0.3f, s * (holeL / 2 + 0.3f)); w2.transform.localRotation = Quaternion.identity;
        }
        for (int k = 0; k < 8; k++) { var r = Rock(Vector3.zero, Random.Range(1f, 2f), shaftRoot, false); r.transform.localPosition = new Vector3(Random.Range(-1f, 1f) * holeW / 2, -Random.Range(3f, 20f), (k % 2 == 0 ? 1 : -1) * holeL / 2); }
        var collapse = Zone<CaveCollapse>("CaveCollapse", hc + Vector3.up * 1.2f, new Vector3(holeW + 2f, 3.4f, holeL - 0.6f), root, yaw);
        collapse.floorSlab = slab.transform;
        var exit = Zone<SceneExit>("FallIntoTheDeep", hc + Vector3.down * (shaftDepth - 4), new Vector3(holeW + 2, 6, holeL + 2), root, yaw);
        exit.sceneName = SL1;
        // blocked end of the tunnel
        Rock(floor[M - 1] + Vector3.up * 2f, 9f, root);
        Kay("rubble_large", floor[holeB + 3], 0, root);
        PointLight(floor[holeB + 2] + Vector3.up * 2.5f, new Color(0.4f, 0.55f, 1f), 1.2f, 10f, root);
        var warn = Zone<TriggerMessage>("HollowWarning", floor[44] + Vector3.up * 1.5f, new Vector3(9, 4, 3), root);
        warn.toast = "The air grows cold... the ground here sounds hollow.";
        warn.newObjective = "Go deeper into the Old Cave";

        CreatePlayer(new Vector3(0, 0, -7), 0);
        var li = Intro("THE OLD CAVE", "Enter the Old Cave (your torch lights the way — F toggles it)", SL1,
            Pg("The Old Cave", "Three days of travel bring you to the mountain from the map. The Old Cave yawns in front of you, completely dark.\n\nYou light your big torch and step inside."));
        li.grantIfMissing = new[] { "map", "key_sun" };
        li.ambientRumble = 0.6f;
        SaveScene(SCave);
    }

    // =================================================================== LEVEL 1: the Fan Tower
    const float RoomR = 22f, FanR = 8f, TowerR = 1.8f;
    static Vector3 Dir(float deg) => new Vector3(Mathf.Cos(deg * Mathf.Deg2Rad), 0, Mathf.Sin(deg * Mathf.Deg2Rad));

    static void BuildLevel1()
    {
        var root = NewScene("Level1_FanTower", new Color(0.07f, 0.08f, 0.09f), new Color(0.02f, 0.05f, 0.03f), 0.012f);
        int segs = 35; float segDeg = 360f / segs;
        float door1Deg = 0, door2Deg = 29 * segDeg;
        float topY = 18f;
        float exitY = 8f;

        // --- ring wall of the giant room
        var wallRoot = new GameObject("RingWall").transform; wallRoot.SetParent(root, false);
        for (int k = 0; k < segs; k++)
        {
            float deg = k * segDeg;
            Vector3 d = Dir(deg), tan = new Vector3(-d.z, 0, d.x);
            if (k == 0 || k == 29) continue;
            for (float y = -wallH; y < topY; y += wallH)
                KayWall((k + (int)(y / wallH)) % 5 == 0 ? "wall_cracked" : (k % 7 == 3 ? "wall_pillar" : "wall"), d * RoomR + Vector3.up * y, tan, wallRoot);
        }
        DoorColumn(root, door1Deg, 0f, -wallH, topY, "Door1_Sun", new[] { "key_sun" }, false,
            "The Sun Key turns in the lock. Deep below, ancient gears groan...",
            "Climb the Tower of Turning Fans and reach the high ledge");
        DoorColumn(root, door2Deg, exitY, -wallH, topY, "Door2_Eclipse", new[] { "key_sun", "key_moon" }, true,
            "You press the Sun Key and the Moon Key together. They lock into one: the ECLIPSE SIGIL. The great door shudders and sinks into the floor.",
            "Enter the Labyrinth beyond the Eclipse Door");

        // --- ceiling
        MeshObj("Ceiling", SaveMesh(DiscMesh(RoomR + 1, 48, 6), "l1_ceiling"), rockDarkMat, new Vector3(0, topY, 0), Quaternion.Euler(180, 0, 0), Vector3.one, root, false);

        // --- acid lake
        var acid = MeshObj("AcidLake", SaveMesh(DiscMesh(RoomR + 0.5f, 64, 8), "l1_acid"), acidMat, new Vector3(0, -2.5f, 0), Quaternion.identity, Vector3.one, root, false);
        acid.AddComponent<UVScroller>();
        Particles("AcidBubbles", new Vector3(0, -2.4f, 0), root, new Color(0.5f, 1f, 0.3f, 0.7f), 60, 1.5f, 0.8f, 0.18f, new Vector3(36, 36, 0.1f), Vector3.up);
        Particles("AcidSteam", new Vector3(0, -1.8f, 0), root, new Color(0.3f, 0.8f, 0.2f, 0.12f), 15, 6f, 0.4f, 2.5f, new Vector3(36, 36, 1f), Vector3.up);
        for (int i = 0; i < 5; i++) PointLight(Dir(i * 72 + 36) * 12 + Vector3.up * -1.2f, new Color(0.35f, 1f, 0.25f), 3.5f, 16f, root);
        var kill = Zone<KillZone>("AcidKill", new Vector3(0, -12f, 0), new Vector3(RoomR * 2 + 4, 19f, RoomR * 2 + 4), root);
        kill.message = "Sssss! The acid burns — back to the last checkpoint.";

        // --- tower
        var tower = Prim(PrimitiveType.Cylinder, new Vector3(0, (topY - 6) / 2f, 0), new Vector3(TowerR * 2, (topY + 6) / 2f, TowerR * 2), Quaternion.identity, stoneMat, root, true, "Tower");
        for (int i = 0; i < 5; i++) Prim(PrimitiveType.Cylinder, new Vector3(0, -1 + i * 3.5f, 0), new Vector3(TowerR * 2.3f, 0.15f, TowerR * 2.3f), Quaternion.identity, brassMat, root, false, "TowerRing");
        var shaft = new GameObject("TowerSkyLight").AddComponent<Light>(); shaft.transform.SetParent(root, false);
        shaft.type = LightType.Spot; shaft.transform.position = new Vector3(0, topY - 0.5f, 0); shaft.transform.rotation = Quaternion.Euler(90, 0, 0);
        shaft.spotAngle = 70; shaft.range = 30; shaft.intensity = 6; shaft.color = new Color(0.55f, 0.7f, 1f);

        // --- fans
        FanPlatform checkpointFan = null;
        for (int i = 0; i < 6; i++)
        {
            float deg = i * 60f; Vector3 d = Dir(deg);
            float fanTop = 1.0f + 1.25f * i;
            var mover = new GameObject("Fan_" + (i + 1)).transform; mover.SetParent(root, false);
            mover.position = d * FanR + Vector3.up * (fanTop - 0.175f);
            var fan = mover.gameObject.AddComponent<FanPlatform>();
            fan.moveOffset = -d * 1.1f + Vector3.up * 0.5f;
            fan.holdTime = 1.4f + (i % 3) * 0.3f; fan.moveTime = 1.0f; fan.phase = i * 1.3f;
            fan.spinSpeed = (i % 2 == 0 ? 1 : -1) * (32f + i * 5f);
            var arm = Prim(PrimitiveType.Cube, Vector3.zero, new Vector3(0.45f, 0.45f, FanR), Quaternion.identity, brassMat, mover, false, "Arm");
            arm.transform.position = mover.position - d * (FanR / 2f) + Vector3.down * 0.5f; arm.transform.rotation = Quaternion.LookRotation(d);
            var spin = new GameObject("Spinner").transform; spin.SetParent(mover, false);
            spin.gameObject.AddComponent<CarryPlatform>();
            var rb = spin.gameObject.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
            fan.spinner = spin;
            var cylMesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
            MeshObj("Hub", cylMesh, brassMat, mover.position, Quaternion.identity, new Vector3(1.9f, 0.35f, 1.9f), spin, true, true);
            MeshObj("HubRune", cylMesh, i == 3 ? goldMat : Emissive("RuneCyan", new Color(0.3f, 0.9f, 1f)), mover.position + Vector3.up * 0.36f, Quaternion.identity, new Vector3(0.7f, 0.02f, 0.7f), spin, false);
            for (int b = 0; b < 3; b++)
            {
                float bd = b * 120f + i * 17f; Vector3 bdir = Dir(bd);
                var blade = Prim(PrimitiveType.Cube, mover.position + bdir * 2.2f, new Vector3(1.5f, 0.35f, 2.7f), Quaternion.LookRotation(bdir), brassMat, spin, true, "Blade");
                Prim(PrimitiveType.Cube, mover.position + bdir * 3.5f + Vector3.up * 0.18f, new Vector3(1.5f, 0.04f, 0.15f), Quaternion.LookRotation(bdir), Emissive("RuneCyan", new Color(0.3f, 0.9f, 1f)), spin, false, "BladeTip");
            }
            var l = PointLight(mover.position + Vector3.up * 1.2f, new Color(0.35f, 0.85f, 1f), 1.2f, 5f, mover);
            if (i == 3) checkpointFan = fan;
        }
        // checkpoint on fan 4's hub (moves with it)
        {
            var hub = checkpointFan.spinner;
            var cp = Zone<Checkpoint>("Checkpoint_Fan4", hub.position + Vector3.up * 0.9f, new Vector3(1.9f, 1.4f, 1.9f), hub);
            cp.anchor = hub; cp.announce = true;
            var sp = new GameObject("Spawn").transform; sp.SetParent(hub, false); sp.position = checkpointFan.transform.position + Vector3.up * 0.5f; cp.spawnPoint = sp;
        }

        // --- start ledge (inside Door1) and exit ledge (Door2)
        Ledge(root, door1Deg, 0f, "StartLedge");
        Ledge(root, door2Deg, exitY, "ExitLedge");
        var cpStart = Zone<Checkpoint>("Checkpoint_Start", Dir(door1Deg) * 17f + Vector3.up * 1f, new Vector3(4, 2, 4), root);
        cpStart.announce = false;
        var sp0 = new GameObject("StartSpawn").transform; sp0.SetParent(root, false); sp0.SetPositionAndRotation(Dir(door1Deg) * 17f + Vector3.up * 0.2f, Quaternion.LookRotation(-Dir(door1Deg))); cpStart.spawnPoint = sp0;
        var cpExit = Zone<Checkpoint>("Checkpoint_Top", Dir(door2Deg) * 16f + Vector3.up * (exitY + 1), new Vector3(4, 2, 6), root);
        var sp1 = new GameObject("TopSpawn").transform; sp1.SetParent(root, false); sp1.SetPositionAndRotation(Dir(door2Deg) * 16f + Vector3.up * (exitY + 0.2f), Quaternion.LookRotation(Dir(door2Deg))); cpExit.spawnPoint = sp1;
        var tip = Zone<TriggerMessage>("FanHint", Dir(door1Deg) * 14f + Vector3.up * 1f, new Vector3(4, 2, 4), root);
        tip.toast = "Time your jumps! The fans spin, rise and pull back on a rhythm. Falling = acid.";

        // Moon Key on a pedestal at the top
        Vector3 pedPos = Dir(door2Deg) * 18.5f + Vector3.up * exitY;
        var ped = Kay("pillar", pedPos, 0, root, true, 0.45f);
        float pedTop = ped ? RendBounds(ped).max.y : pedPos.y + 1.5f;
        var keyHolder = new GameObject("MoonKey").transform; keyHolder.SetParent(root, false);
        keyHolder.position = new Vector3(pedPos.x, pedTop + 0.6f, pedPos.z);
        var moonKey = Kay("key", keyHolder.position, 0, keyHolder, false, 1f, 0.6f, silverMat);
        keyHolder.gameObject.AddComponent<Spinner>();
        PointLight(keyHolder.position + Vector3.up * 0.5f, new Color(0.6f, 0.75f, 1f), 2.5f, 7f, root);
        var pk = keyHolder.gameObject.AddComponent<PickupItem>();
        pk.prompt = "Take the Moon Key"; pk.itemIds = new[] { "key_moon" }; pk.toast = "Got: Moon Key";
        pk.newObjective = "Open the Eclipse Door using BOTH keys";
        pk.story = new List<StoryPage> { Pg("The Moon Key", "A cold silver key, shaped like a crescent moon. Its teeth fit perfectly into the notches of your Sun Key.\n\nThe door behind the pedestal has an eclipse carved into it...") };

        // torches around the room
        for (int i = 0; i < 8; i++)
        {
            float deg = i * 45 + 22.5f; Vector3 d = Dir(deg);
            Torch(d * (RoomR - 0.6f) + Vector3.up * 3f, root, true, -d);
        }
        Torch(Dir(door2Deg) * (RoomR - 0.6f) + Dir(door2Deg + 90) * 2.5f + Vector3.up * (exitY + 1.5f), root, true, -Dir(door2Deg));

        // antechamber outside Door1 (where the player lands)
        var ante = new List<Vector3>(); for (int i = 0; i < 14; i++) ante.Add(Dir(door1Deg) * (RoomR + 0.6f + i * 1.25f));
        ante.Reverse();
        MeshObj("Antechamber", SaveMesh(TunnelMesh(ante, i => 3.6f, i => 3.4f, 1.7f, 24, 0.5f, i => false, out _), "l1_ante"), rockMat, Vector3.zero, Quaternion.identity, Vector3.one, root);
        Rock(ante[0] + Dir(door1Deg) * 2.5f + Vector3.up * 1.5f, 7f, root);
        Kay("rubble_large", Dir(door1Deg) * 31f + Vector3.forward * 2.2f, 0, root);
        Rock(Dir(door1Deg) * 30f + Vector3.back * 2.5f, 1.0f, root);
        Torch(Dir(door1Deg) * (RoomR + 1.2f) + Vector3.forward * 2.6f + Vector3.up * 1.8f, root, true, Vector3.back);

        // corridor beyond Door2 to the labyrinth
        var corr = new List<Vector3>(); for (int i = 0; i < 9; i++) corr.Add(Dir(door2Deg) * (RoomR + 0.6f + i * 1.25f) + Vector3.up * exitY);
        MeshObj("ExitTunnel", SaveMesh(TunnelMesh(corr, i => 3.2f, i => 3f, 1.7f, 24, 0.4f, i => false, out _), "l1_exit"), rockMat, Vector3.zero, Quaternion.identity, Vector3.one, root);
        var ex = Zone<SceneExit>("ExitToLabyrinth", corr[7] + Vector3.up * 1.5f, new Vector3(5, 4, 2.5f), root, -door2Deg + 90);
        ex.sceneName = SL2;

        CreatePlayer(Dir(door1Deg) * 28f + Vector3.up * 3f, -90);
        var li = Intro("LEVEL 1 — THE TOWER OF TURNING FANS", "Open the great door with the Sun Key (press E at the door)", SL2,
            Pg("Into the Deep", "The ground gave way and you fell... and fell... into a pile of soft rubble far beneath the mountain.\n\nYour torch still works. In front of you stands a massive stone door carved with a golden SUN."),
            Pg("Level 1", "Beyond the door lies a giant cavern. A stone tower rises from a lake of bubbling acid, circled by six great bronze FANS that spin, rise and pull back on a steady rhythm.\n\nRide the fans up to the high ledge. Somewhere up there is the second key."));
        li.grantIfMissing = new[] { "map", "key_sun" };
        li.ambientRumble = 0.7f;
        SaveScene(SL1);
    }

    static void Ledge(Transform root, float deg, float topY, string name)
    {
        Vector3 d = Dir(deg);
        float inner = 12f, outer = RoomR + 0.6f;
        var go = Prim(PrimitiveType.Cube, d * ((inner + outer) / 2f) + Vector3.up * (topY - 0.6f), new Vector3(5f, 1.2f, outer - inner), Quaternion.LookRotation(d), stoneMat, root, true, name);
        Prim(PrimitiveType.Cube, d * (inner + 0.1f) + Vector3.up * (topY - 0.02f), new Vector3(5f, 0.06f, 0.2f), Quaternion.LookRotation(d), Emissive("EdgeGold", new Color(1f, 0.7f, 0.25f), 1.5f), root, false, name + "_Edge");
        for (int i = 0; i < 3; i++) Rock(d * (inner + 1f + i * 3.5f) + Vector3.up * (topY - 2.2f), 2.2f, root, false);
    }

    /// Replaces one wall column with a stone door frame + sliding slab.
    static void DoorColumn(Transform root, float deg, float baseY, float fromY, float toY, string name, string[] keys, bool eclipse, string openText, string objective)
    {
        Vector3 d = Dir(deg); Vector3 tan = new Vector3(-d.z, 0, d.x);
        var rot = Quaternion.LookRotation(d);
        Vector3 c = d * RoomR;
        float openW = 3.4f, openH = 3.6f, colW = 4.1f, thick = 1.0f;
        var fr = new GameObject(name).transform; fr.SetParent(root, false); fr.SetPositionAndRotation(c, rot);
        void Block(float x0, float x1, float y0, float y1)
        {
            if (y1 - y0 < 0.01f) return;
            var b = Prim(PrimitiveType.Cube, Vector3.zero, new Vector3(x1 - x0, y1 - y0, thick), Quaternion.identity, stoneMat, fr, true, "Frame");
            b.transform.localPosition = new Vector3((x0 + x1) / 2f, (y0 + y1) / 2f, 0); b.transform.localRotation = Quaternion.identity;
        }
        Block(-colW / 2, -openW / 2, fromY, toY); Block(openW / 2, colW / 2, fromY, toY);
        Block(-openW / 2, openW / 2, fromY, baseY); Block(-openW / 2, openW / 2, baseY + openH, toY);
        var lintel = Prim(PrimitiveType.Cube, Vector3.zero, new Vector3(openW + 0.8f, 0.25f, thick + 0.3f), Quaternion.identity, goldMat, fr, false, "Lintel");
        lintel.transform.localPosition = new Vector3(0, baseY + openH + 0.1f, 0); lintel.transform.localRotation = Quaternion.identity;
        // slab
        var slab = new GameObject("Slab").transform; slab.SetParent(fr, false); slab.localPosition = new Vector3(0, baseY + openH / 2f, 0); slab.localRotation = Quaternion.identity;
        var sb = Prim(PrimitiveType.Cube, Vector3.zero, new Vector3(openW, openH, 0.5f), Quaternion.identity, stoneMat, slab, true, "SlabStone");
        sb.transform.localPosition = Vector3.zero; sb.transform.localRotation = Quaternion.identity;
        var cyl = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        foreach (float side in new[] { -1f, 1f })
        {
            var em = new GameObject("Emblem").transform; em.SetParent(slab, false); em.localPosition = new Vector3(0, 0.3f, side * 0.27f); em.localRotation = Quaternion.identity;
            if (!eclipse)
            {
                MeshObj("Sun", cyl, goldMat, Vector3.zero, Quaternion.identity, Vector3.one, em, false).transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(90, 0, 0));
                em.GetChild(0).localScale = new Vector3(1.1f, 0.03f, 1.1f);
                for (int r = 0; r < 12; r++) { var ray = Prim(PrimitiveType.Cube, Vector3.zero, new Vector3(0.1f, 0.35f, 0.04f), Quaternion.identity, goldMat, em, false, "Ray"); float a = r * 30; ray.transform.localPosition = Quaternion.Euler(0, 0, a) * Vector3.up * 0.8f; ray.transform.localRotation = Quaternion.Euler(0, 0, a); }
            }
            else
            {
                var s1 = MeshObj("Sun", cyl, goldMat, Vector3.zero, Quaternion.identity, Vector3.one, em, false); s1.transform.SetLocalPositionAndRotation(new Vector3(-0.35f, 0, 0), Quaternion.Euler(90, 0, 0)); s1.transform.localScale = new Vector3(1f, 0.03f, 1f);
                var s2 = MeshObj("Moon", cyl, silverMat, Vector3.zero, Quaternion.identity, Vector3.one, em, false); s2.transform.SetLocalPositionAndRotation(new Vector3(0.35f, 0, side * 0.02f), Quaternion.Euler(90, 0, 0)); s2.transform.localScale = new Vector3(1f, 0.035f, 1f);
            }
        }
        var door = fr.gameObject.AddComponent<LockedDoor>();
        door.prompt = eclipse ? "Open the Eclipse Door" : "Open the Sun Door"; door.range = 3.2f;
        door.requiredItems = keys; door.slab = slab; door.openDepth = openH + 0.2f; door.newObjective = objective;
        door.openStory = new List<StoryPage> { Pg(eclipse ? "The Eclipse Sigil" : "The Sun Door", openText) };
        // the interactable point should be at door height
        var col = fr.gameObject; col.transform.position = c; // keep frame at wall
        var focus = new GameObject("Focus"); focus.transform.SetParent(fr, false);
        fr.position = c; // LockedDoor distance uses fr.position (at y=0) -> shift frame pivot up to door centre
        foreach (Transform ch in fr) ch.localPosition -= new Vector3(0, baseY + 1.2f, 0);
        fr.position = c + Vector3.up * (baseY + 1.2f);
        Object.DestroyImmediate(focus);
    }

    // =================================================================== LEVEL 2: the Proverb Labyrinth
    static readonly string[] MazeRows = {
        // x: 0123456789012   (row index = z, listed from z=12 down to z=0)
        "###########.#",   // z=12  exit (11,12)
        "#######...#.#",   // z=11  hmm replaced below
        "#.#######.###",
        "#.#.......###",
        "####.########",
        "##.#.#.#.####",
        "##.........##",
        "##########.##",
        "##########.##",
        "##########.##",
        "##.........##",
        "######.######",
        "######.######",
    };

    static void BuildLevel2()
    {
        var root = NewScene("Level2_Labyrinth", new Color(0.05f, 0.05f, 0.07f), new Color(0.01f, 0.012f, 0.02f), 0.03f);
        // explicit floor cells (x,z)
        var cells = new HashSet<Vector2Int>();
        void Add(params (int, int)[] cs) { foreach (var c in cs) cells.Add(new Vector2Int(c.Item1, c.Item2)); }
        Add((6, 0), (6, 1), (6, 2), (6, 3));                       // start corridor -> junction A
        Add((5, 3), (4, 3), (3, 3), (2, 3));                       // A west: setting sun (trap)
        Add((7, 3), (8, 3), (9, 3), (10, 3), (10, 4), (10, 5), (10, 6)); // A east: rising sun
        for (int x = 2; x <= 9; x++) Add((x, 6));                  // serpent corridor
        Add((8, 7), (6, 7), (2, 7));                               // 1, 2, 4 coils (traps)
        Add((4, 7), (4, 8), (4, 9));                               // 3 coils -> fountain -> owl junction
        Add((3, 9), (2, 9), (1, 9), (1, 10));                      // owl side (trap)
        for (int x = 5; x <= 9; x++) Add((x, 9));                  // away from the owl
        Add((9, 10), (9, 11));                                     // to final junction
        Add((8, 11), (7, 11));                                     // setting sun (trap)
        Add((10, 11), (11, 11), (11, 12));                         // rising sun -> treasure
        var traps = new HashSet<Vector2Int> { new Vector2Int(2, 3), new Vector2Int(8, 7), new Vector2Int(6, 7), new Vector2Int(2, 7), new Vector2Int(1, 10), new Vector2Int(7, 11) };
        const float S = 4f;
        Vector3 C(int x, int z) => new Vector3(x * S, 0, z * S);
        bool Floor(int x, int z) => cells.Contains(new Vector2Int(x, z));

        var mazeRoot = new GameObject("Maze").transform; mazeRoot.SetParent(root, false);
        var dirs = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        var posts = new HashSet<Vector2Int>();
        foreach (var c in cells)
        {
            var pos = C(c.x, c.y);
            if (traps.Contains(c))
            {
                var trapRoot = new GameObject("TrapTile").transform; trapRoot.SetParent(mazeRoot, false); trapRoot.position = pos;
                Kay("floor_tile_large", pos, 0, trapRoot, false, 1, S);
                var col = trapRoot.gameObject.AddComponent<BoxCollider>(); col.center = new Vector3(0, -0.25f, 0); col.size = new Vector3(S, 0.5f, S);
                var tz = Zone<TrapFloor>("TrapTrigger", pos + Vector3.up * 1f, new Vector3(S * 0.7f, 2f, S * 0.7f), mazeRoot);
                tz.tile = trapRoot;
            }
            else
            {
                Kay(Random.value < 0.25f ? "floor_tile_large_rocks" : "floor_tile_large", pos, 0, mazeRoot, false, 1, S);
                var fc = Prim(PrimitiveType.Cube, pos + Vector3.down * 0.25f, new Vector3(S, 0.5f, S), Quaternion.identity, stoneMat, mazeRoot, true, "FloorCol");
                fc.GetComponent<Renderer>().enabled = false;
            }
            foreach (var d in dirs)
            {
                if (Floor(c.x + d.x, c.y + d.y)) continue;
                var edge = pos + new Vector3(d.x, 0, d.y) * (S / 2f);
                Vector3 along = new Vector3(d.y, 0, d.x);
                string piece = (c == new Vector2Int(6, 0) && d == Vector2Int.down) ? "wall_gated" : (Random.value < 0.2f ? "wall_cracked" : "wall");
                KayWall(piece, edge, along, mazeRoot);
                KayWall("wall", edge + Vector3.up * wallH, along, mazeRoot);
                // corner posts
                var e0 = new Vector2Int(c.x * 2 + d.x + d.y, c.y * 2 + d.y + d.x); var e1 = new Vector2Int(c.x * 2 + d.x - d.y, c.y * 2 + d.y - d.x);
                posts.Add(e0); posts.Add(e1);
            }
        }
        foreach (var p in posts) Prim(PrimitiveType.Cube, new Vector3(p.x * S / 2f, wallH, p.y * S / 2f), new Vector3(0.7f, wallH * 2, 0.7f), Quaternion.identity, stoneMat, mazeRoot, true, "Post");
        Prim(PrimitiveType.Cube, new Vector3(6 * S, wallH * 2 + 0.1f, 6 * S), new Vector3(14 * S, 0.2f, 14 * S), Quaternion.identity, rockDarkMat, root, false, "Ceiling");
        var kill = Zone<KillZone>("PitKill", new Vector3(6 * S, -14, 6 * S), new Vector3(15 * S, 20, 15 * S), root);
        kill.message = "The floor gave way! The labyrinth punishes the careless...";
        kill.sound = Sfx.Rumble;

        // symbols (mounted on the far wall of a branch's first cell)
        void Sym(int x, int z, Vector2Int wallDir, System.Action<Transform> build, Color light)
        {
            Vector3 d = new Vector3(wallDir.x, 0, wallDir.y);
            var t = new GameObject("Symbol").transform; t.SetParent(root, false);
            t.SetPositionAndRotation(C(x, z) + d * (S / 2f - 0.35f) + Vector3.up * 2.3f, Quaternion.LookRotation(d));
            var plaque = Prim(PrimitiveType.Cube, Vector3.zero, new Vector3(1.7f, 1.7f, 0.1f), Quaternion.identity, rockDarkMat, t, false, "Plaque");
            plaque.transform.localPosition = new Vector3(0, 0, 0.06f); plaque.transform.localRotation = Quaternion.identity;
            build(t);
            PointLight(t.position - d * 1.2f, light, 1.6f, 4.5f, root);
        }
        var N = Vector2Int.up;
        Sym(7, 3, N, RisingSun, new Color(1f, 0.8f, 0.4f));
        Sym(5, 3, N, SettingSun, new Color(1f, 0.3f, 0.2f));
        Sym(8, 7, N, t => Serpent(t, 1), new Color(0.4f, 1f, 0.4f));
        Sym(6, 7, N, t => Serpent(t, 2), new Color(0.4f, 1f, 0.4f));
        Sym(4, 8, Vector2Int.left, t => Serpent(t, 3), new Color(0.4f, 1f, 0.4f));
        Sym(2, 7, N, t => Serpent(t, 4), new Color(0.4f, 1f, 0.4f));
        Sym(3, 9, N, Owl, new Color(1f, 0.9f, 0.5f));
        Sym(10, 11, N, RisingSun, new Color(1f, 0.8f, 0.4f));
        Sym(8, 11, N, SettingSun, new Color(1f, 0.3f, 0.2f));

        // fountain where "the water speaks"
        var fpos = C(4, 8);
        var cyl = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
        MeshObj("FountainBasin", cyl, stoneMat, fpos + new Vector3(1.1f, 0.35f, 0), Quaternion.identity, new Vector3(1.4f, 0.35f, 1.4f), root, true, true);
        MeshObj("FountainWater", cyl, waterMat, fpos + new Vector3(1.1f, 0.71f, 0), Quaternion.identity, new Vector3(1.2f, 0.02f, 1.2f), root, false);
        Particles("FountainSpray", fpos + new Vector3(1.1f, 0.8f, 0), root, new Color(0.6f, 0.8f, 1f, 0.8f), 30, 1f, 2.2f, 0.07f, new Vector3(0.1f, 0.1f, 0.1f), Vector3.up, 0.6f);
        PointLight(fpos + new Vector3(1.1f, 1.5f, 0), new Color(0.4f, 0.7f, 1f), 2f, 6f, root);
        var water = Zone<TriggerMessage>("WaterSpeaks", fpos + Vector3.up, new Vector3(3, 2, 3), root);
        water.toast = "The water whispers: \"Three coils... you counted well. Now beware the watching owl.\"";
        water.playSound = true;

        // checkpoints
        void CP(int x, int z, float yaw) { var cp = Zone<Checkpoint>("Checkpoint", C(x, z) + Vector3.up, new Vector3(3, 2, 3), root); var sp = new GameObject("Spawn").transform; sp.SetParent(cp.transform, false); sp.SetPositionAndRotation(C(x, z) + Vector3.up * 0.1f, Quaternion.Euler(0, yaw, 0)); cp.spawnPoint = sp; }
        CP(6, 0, 0); CP(10, 6, -90); CP(4, 8, 0); CP(9, 11, 0);

        // torches at junctions
        Torch(C(6, 3) + new Vector3(0, 1.8f, 1.6f), root, true, Vector3.back);
        Torch(C(10, 6) + new Vector3(1.6f, 1.8f, 0), root, true, Vector3.left);
        Torch(C(4, 9) + new Vector3(0, 1.8f, 1.6f), root, true, Vector3.back);
        Torch(C(9, 11) + new Vector3(0, 1.8f, 1.6f), root, true, Vector3.back);
        Torch(C(6, 0) + new Vector3(-1.6f, 1.8f, 0), root, true, Vector3.right);

        // treasure vault at (11,12)
        var tpos = C(11, 12);
        var chest = Kay("chest_gold", tpos + Vector3.forward * 0.6f, 180, root);
        Kay("coin_stack_large", tpos + new Vector3(-1.2f, 0, 0.8f), 0, root, false);
        Kay("coin_stack_large", tpos + new Vector3(1.2f, 0, 1.0f), 40, root, false);
        var glow = PointLight(tpos + Vector3.up * 1.5f, new Color(1f, 0.75f, 0.3f), 4f, 10f, root, true);
        var tc = new GameObject("TreasureChest"); tc.transform.SetParent(root, false); tc.transform.position = tpos + Vector3.up * 0.8f;
        var chestI = tc.AddComponent<TreasureChest>();
        chestI.prompt = "Open the golden chest";
        chestI.title = "LEVEL 2 COMPLETE!";
        chestI.body = "Inside the chest lies a golden fragment of a second map... and a whisper of three more trials below.\n\nYou have finished the Adventures demo (Levels 1 & 2).\nLevels 3, 4 and 5 are coming soon!";
        chestI.nextScene = SIntro;
        Particles("GoldSparkle", tpos + Vector3.up * 1.2f, root, new Color(1f, 0.85f, 0.3f), 12, 2f, 0.3f, 0.08f, new Vector3(2, 1, 2), Vector3.up);

        CreatePlayer(C(6, 0) + Vector3.up * 0.1f, 0);
        var li = Intro("LEVEL 2 — THE PROVERB LABYRINTH", "Solve the labyrinth using the proverb (TAB to re-read it)", SIntro,
            Pg("The Proverb Labyrinth", "Past the Eclipse Door the tunnel opens into a maze of narrow stone halls. Carved above the entrance is a proverb. The Keeper wrote that it is the ONLY safe way through — the wrong paths collapse into the pit."),
            Pg("The Keeper's Proverb", "\"Follow the SUN that RISES, and never the one that SETS — the sinking sun leads only to the dark below.\n\nWhere the SERPENT coils THREE times, the WATER will speak. Count well: one coil too few or too many, and the earth will swallow you.\n\nWhere the OWL keeps watch, turn your BACK upon its eyes.\n\nAnd when the sun rises once more, the gold of the kings is near.\"\n\n(Press TAB at any time to read this again.)"));
        li.rereadWithTab = true;
        li.ambientRumble = 0.5f;
        SaveScene(SL2);
    }

    // ---------- symbol builders (local space: face toward -Z, i.e. into the corridor)
    static Transform Part(Transform parent, PrimitiveType t, Vector3 lp, Vector3 ls, Quaternion lr, Material m)
    {
        var g = Prim(t, Vector3.zero, Vector3.one, Quaternion.identity, m, parent, false, t.ToString());
        g.transform.localPosition = lp; g.transform.localRotation = lr; g.transform.localScale = ls; return g.transform;
    }
    static Quaternion Face => Quaternion.Euler(90, 0, 0);

    static void RisingSun(Transform t)
    {
        var gold = goldMat;
        Part(t, PrimitiveType.Cube, new Vector3(0, -0.45f, -0.02f), new Vector3(1.3f, 0.08f, 0.06f), Quaternion.identity, Emissive("SymGold", new Color(1f, 0.7f, 0.2f)));
        Part(t, PrimitiveType.Cylinder, new Vector3(0, 0.05f, -0.04f), new Vector3(0.65f, 0.02f, 0.65f), Face, gold);
        for (int i = 0; i <= 8; i++) { float a = i * 22.5f; Part(t, PrimitiveType.Cube, new Vector3(0, 0.05f, -0.04f) + Quaternion.Euler(0, 0, a - 90) * Vector3.up * 0.58f, new Vector3(0.06f, 0.2f, 0.04f), Quaternion.Euler(0, 0, a - 90), gold); }
        Part(t, PrimitiveType.Cube, new Vector3(0, -0.62f, -0.04f), new Vector3(0.06f, 0.2f, 0.04f), Quaternion.identity, gold); // up arrow stem
        Part(t, PrimitiveType.Cube, new Vector3(-0.06f, -0.55f, -0.04f), new Vector3(0.05f, 0.14f, 0.04f), Quaternion.Euler(0, 0, -45), gold);
        Part(t, PrimitiveType.Cube, new Vector3(0.06f, -0.55f, -0.04f), new Vector3(0.05f, 0.14f, 0.04f), Quaternion.Euler(0, 0, 45), gold);
    }

    static void SettingSun(Transform t)
    {
        var red = Emissive("SymRed", new Color(0.9f, 0.15f, 0.05f), 2.5f);
        Part(t, PrimitiveType.Cube, new Vector3(0, -0.05f, -0.02f), new Vector3(1.3f, 0.08f, 0.06f), Quaternion.identity, Emissive("SymGold", new Color(1f, 0.7f, 0.2f)));
        var hd = MeshObj("HalfSun", halfDiscMesh, red, Vector3.zero, Quaternion.identity, Vector3.one, t, false);
        hd.transform.localPosition = new Vector3(0, -0.01f, -0.05f); hd.transform.localRotation = Quaternion.Euler(0, 180, 0); hd.transform.localScale = new Vector3(0.9f, 0.9f, 1);
        Part(t, PrimitiveType.Cube, new Vector3(0, -0.35f, -0.04f), new Vector3(0.06f, 0.25f, 0.04f), Quaternion.identity, red); // down arrow
        Part(t, PrimitiveType.Cube, new Vector3(-0.06f, -0.45f, -0.04f), new Vector3(0.05f, 0.14f, 0.04f), Quaternion.Euler(0, 0, 45), red);
        Part(t, PrimitiveType.Cube, new Vector3(0.06f, -0.45f, -0.04f), new Vector3(0.05f, 0.14f, 0.04f), Quaternion.Euler(0, 0, -45), red);
    }

    static void Serpent(Transform t, int coils)
    {
        var green = Emissive("SymGreen", new Color(0.2f, 0.9f, 0.3f), 2.2f);
        float spacing = 0.34f, start = -(coils - 1) * spacing / 2f;
        for (int c = 0; c < coils; c++)
            for (int k = 0; k < 12; k++) { float a = k * 30; Part(t, PrimitiveType.Cube, new Vector3(start + c * spacing, 0, -0.04f) + Quaternion.Euler(0, 0, a) * Vector3.up * 0.15f, new Vector3(0.09f, 0.05f, 0.04f), Quaternion.Euler(0, 0, a), green); }
        float end = start + (coils - 1) * spacing;
        Part(t, PrimitiveType.Sphere, new Vector3(end + 0.3f, 0.12f, -0.05f), new Vector3(0.2f, 0.14f, 0.06f), Quaternion.identity, green);
        Part(t, PrimitiveType.Sphere, new Vector3(end + 0.34f, 0.15f, -0.09f), Vector3.one * 0.04f, Quaternion.identity, Emissive("SymRed", new Color(0.9f, 0.15f, 0.05f), 2.5f));
        Part(t, PrimitiveType.Cube, new Vector3(start - 0.3f, -0.12f, -0.04f), new Vector3(0.25f, 0.04f, 0.04f), Quaternion.Euler(0, 0, 20), green);
        // numeral dots under the serpent (numbers hint)
        for (int c = 0; c < coils; c++) Part(t, PrimitiveType.Sphere, new Vector3(start + c * spacing, -0.5f, -0.05f), Vector3.one * 0.09f, Quaternion.identity, green);
    }

    static void Owl(Transform t)
    {
        var brown = Mat("OwlFeathers", new Color(0.35f, 0.22f, 0.12f), null, 0.2f, 0, new Color(0.15f, 0.08f, 0.03f));
        var eye = Emissive("OwlEye", new Color(1f, 0.85f, 0.3f), 3f);
        Part(t, PrimitiveType.Cylinder, new Vector3(0, 0, -0.03f), new Vector3(1.1f, 0.02f, 1.2f), Face, brown);
        foreach (int s in new[] { -1, 1 })
        {
            Part(t, PrimitiveType.Cylinder, new Vector3(s * 0.24f, 0.1f, -0.06f), new Vector3(0.38f, 0.02f, 0.38f), Face, eye);
            Part(t, PrimitiveType.Cylinder, new Vector3(s * 0.24f, 0.1f, -0.08f), new Vector3(0.14f, 0.02f, 0.14f), Face, blackMat);
            Part(t, PrimitiveType.Cube, new Vector3(s * 0.35f, 0.58f, -0.05f), new Vector3(0.12f, 0.3f, 0.04f), Quaternion.Euler(0, 0, -s * 25), brown);
        }
        Part(t, PrimitiveType.Cube, new Vector3(0, -0.12f, -0.07f), new Vector3(0.1f, 0.1f, 0.04f), Quaternion.Euler(0, 0, 45), Emissive("SymGold", new Color(1f, 0.7f, 0.2f)));
    }
}
