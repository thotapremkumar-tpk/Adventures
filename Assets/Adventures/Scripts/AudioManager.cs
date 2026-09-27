using System;
using System.Collections.Generic;
using UnityEngine;

namespace Adventures
{
    /// Procedural sound effects so the demo has audio without external files.
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I;
        AudioSource oneShot, ambient;
        readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
        float nextDrip;

        public static AudioManager Ensure()
        {
            if (I == null) { var go = new GameObject("AudioManager"); I = go.AddComponent<AudioManager>(); }
            return I;
        }

        void Awake()
        {
            if (I != null && I != this) { Destroy(gameObject); return; }
            I = this;
            oneShot = gameObject.AddComponent<AudioSource>();
            ambient = gameObject.AddComponent<AudioSource>();
            ambient.loop = true; ambient.volume = 0.5f;
            nextDrip = Time.time + 2f;
        }

        AudioClip Get(Sfx s)
        {
            if (!clips.TryGetValue(s, out var c)) { c = ProcAudio.Make(s); clips[s] = c; }
            return c;
        }

        public void Play(Sfx s, float vol = 1f) { oneShot.PlayOneShot(Get(s), vol); }

        public bool drips;
        public void StartAmbient(float rumble, bool withDrips)
        {
            ambient.clip = ProcAudio.Drone(rumble);
            ambient.Play();
            drips = withDrips;
        }

        void Update()
        {
            if (drips && Time.time > nextDrip)
            {
                nextDrip = Time.time + UnityEngine.Random.Range(1.5f, 5f);
                oneShot.pitch = UnityEngine.Random.Range(0.8f, 1.3f);
                oneShot.PlayOneShot(Get(Sfx.Drip), UnityEngine.Random.Range(0.15f, 0.4f));
                oneShot.pitch = 1f;
            }
        }
    }
}
