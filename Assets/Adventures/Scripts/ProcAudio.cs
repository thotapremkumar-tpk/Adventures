using System;
using System.Collections.Generic;
using UnityEngine;

namespace Adventures
{
    public static class ProcAudio
    {
        const int SR = 44100;

        static AudioClip Build(string name, float seconds, Func<float, float> f)
        {
            int n = Mathf.CeilToInt(seconds * SR);
            var data = new float[n];
            for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)SR), -1f, 1f);
            var clip = AudioClip.Create(name, n, 1, SR, false);
            clip.SetData(data, 0);
            return clip;
        }

        static float Env(float t, float len, float attack = 0.01f) => Mathf.Clamp01(t / attack) * Mathf.Clamp01(1f - t / len);

        public static AudioClip Make(Sfx s)
        {
            var rng = new System.Random((int)s * 97 + 13);
            float lp = 0, lp2 = 0;
            Func<float> white = () => (float)rng.NextDouble() * 2f - 1f;
            switch (s)
            {
                case Sfx.Pickup:
                    return Build("pickup", 0.7f, t =>
                    {
                        float[] notes = { 523f, 659f, 784f, 1046f };
                        int k = Mathf.Min(3, (int)(t / 0.1f));
                        float tt = t - k * 0.1f;
                        return 0.35f * Mathf.Sin(2 * Mathf.PI * notes[k] * t) * Mathf.Exp(-tt * 6f) * Env(t, 0.7f);
                    });
                case Sfx.Success:
                    return Build("success", 1.6f, t => 0.25f * (Mathf.Sin(2 * Mathf.PI * 392 * t) + Mathf.Sin(2 * Mathf.PI * 494 * t) + Mathf.Sin(2 * Mathf.PI * 587 * t)) / 3f * 2f * Env(t, 1.6f, 0.05f));
                case Sfx.Fail:
                    return Build("fail", 0.6f, t => 0.35f * Mathf.Sin(2 * Mathf.PI * (t < 0.25f ? 220 : 165) * t) * Env(t, 0.6f));
                case Sfx.DoorGrind:
                    return Build("door", 2.6f, t => { lp += 0.05f * (white() - lp); return 1.8f * lp * (0.6f + 0.4f * Mathf.Sin(t * 37f)) * Env(t, 2.6f, 0.2f); });
                case Sfx.Rumble:
                    return Build("rumble", 2.8f, t => { lp += 0.02f * (white() - lp); lp2 += 0.05f * (lp - lp2); return 6f * lp2 * Env(t, 2.8f, 0.3f); });
                case Sfx.Drip:
                    return Build("drip", 0.35f, t => { float f = Mathf.Lerp(1800, 500, Mathf.Clamp01(t / 0.08f)); return 0.5f * Mathf.Sin(2 * Mathf.PI * f * t) * Mathf.Exp(-t * 25f) + 0.15f * Mathf.Sin(2 * Mathf.PI * f * (t - 0.12f)) * Mathf.Exp(-Mathf.Max(0, t - 0.12f) * 25f) * (t > 0.12f ? 1 : 0); });
                case Sfx.Splash:
                    return Build("splash", 0.8f, t => { lp += 0.3f * (white() - lp); return 0.8f * lp * Mathf.Exp(-t * 5f); });
                case Sfx.Whoosh:
                    return Build("whoosh", 0.5f, t => { lp += 0.15f * (white() - lp); return 0.9f * lp * Mathf.Sin(Mathf.PI * t / 0.5f); });
                case Sfx.Page:
                    return Build("page", 0.18f, t => { lp += 0.5f * (white() - lp); return 0.5f * lp * Env(t, 0.18f); });
            }
            return Build("silence", 0.1f, t => 0);
        }

        public static AudioClip Drone(float amount)
        {
            var rng = new System.Random(5);
            float lp = 0, lp2 = 0;
            float len = 8f;
            return Build("drone", len, t =>
            {
                lp += 0.01f * (((float)rng.NextDouble() * 2 - 1) - lp);
                lp2 += 0.03f * (lp - lp2);
                float fade = Mathf.Clamp01(t / 0.5f) * Mathf.Clamp01((len - t) / 0.5f);
                return amount * (8f * lp2 + 0.04f * Mathf.Sin(2 * Mathf.PI * 55 * t)) * (0.6f + 0.4f * fade);
            });
        }
    }
}
