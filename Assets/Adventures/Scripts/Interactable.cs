using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    public abstract class Interactable : MonoBehaviour
    {
        public static readonly List<Interactable> All = new List<Interactable>();
        public string prompt = "Interact";
        public float range = 2.8f;
        public virtual bool CanInteract => true;
        protected virtual void OnEnable() { All.Add(this); }
        protected virtual void OnDisable() { All.Remove(this); }
        public abstract void Interact(PlayerController p);
    }
}
