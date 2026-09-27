using System.Collections;
using UnityEngine;

namespace Adventures
{
    /// A hinged door that swings open with E (optionally only after the player has an item).
    public class SwingDoor : Interactable
    {
        public string requiredItem;
        public string lockedMessage = "It's locked.";
        public float angle = 100f;
        bool open;
        public override bool CanInteract => !open;

        public override void Interact(PlayerController p)
        {
            if (!string.IsNullOrEmpty(requiredItem) && !GameState.Has(requiredItem))
            {
                if (AudioManager.I) AudioManager.I.Play(Sfx.Fail, 0.6f);
                UIManager.I.Toast(lockedMessage);
                return;
            }
            open = true;
            StartCoroutine(Swing(p ? p.transform.position : transform.position - transform.forward));
        }

        IEnumerator Swing(Vector3 playerPos)
        {
            var rs = GetComponentsInChildren<Renderer>();
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
            Vector3 hinge = b.size.x >= b.size.z ? new Vector3(b.min.x, b.min.y, b.center.z) : new Vector3(b.center.x, b.min.y, b.min.z);
            // swing away from the player
            Vector3 c = b.center;
            Vector3 test = Quaternion.AngleAxis(angle, Vector3.up) * (c - hinge) + hinge;
            float sign = Vector3.Distance(test, playerPos) > Vector3.Distance(c, playerPos) ? 1f : -1f;
            foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;
            if (AudioManager.I) AudioManager.I.Play(Sfx.DoorGrind, 0.5f);
            float done = 0;
            while (done < angle)
            {
                float step = Mathf.Min(angle - done, angle * Time.deltaTime / 1.2f);
                transform.RotateAround(hinge, Vector3.up, step * sign);
                done += step;
                yield return null;
            }
            UIManager.I.Toast("The door creaks open. The night air is cold... to the Old Cave!", 3f);
        }
    }
}
