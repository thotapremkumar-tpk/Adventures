using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    /// Box volume (uses a trigger BoxCollider for its shape) that detects the player by position every frame.
    [RequireComponent(typeof(BoxCollider))]
    public abstract class PlayerZone : MonoBehaviour
    {
        protected BoxCollider box;
        bool inside;
        protected virtual void Awake() { box = GetComponent<BoxCollider>(); box.isTrigger = true; }
        protected virtual void Update()
        {
            var p = PlayerController.I; if (!p) return;
            Vector3 local = transform.InverseTransformPoint(p.transform.position + Vector3.up * 0.9f) - box.center;
            Vector3 h = box.size * 0.5f;
            bool now = Mathf.Abs(local.x) <= h.x && Mathf.Abs(local.y) <= h.y && Mathf.Abs(local.z) <= h.z;
            if (now && !inside) OnPlayerEnter(p);
            inside = now;
        }
        protected abstract void OnPlayerEnter(PlayerController p);
    }
}
