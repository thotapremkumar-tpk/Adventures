using UnityEngine;
using UnityEngine.InputSystem;

namespace Adventures
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController I;
        public float walkSpeed = 5.5f, sprintSpeed = 8f, jumpHeight = 1.7f, gravity = -22f, turnSpeed = 14f, killY = -80f;
        public Animator animator;

        CharacterController cc;
        Vector3 velocity;
        float lastGrounded = -10, lastJump = -10;
        Transform platform; Vector3 platformLocal; Quaternion platformRot;
        Vector3 respawnPos; Quaternion respawnRot; Transform respawnAnchor;
        const int Mask = ~(1 << 2);
        public bool Grounded { get; private set; }

        void Awake()
        {
            I = this;
            cc = GetComponent<CharacterController>();
            if (!animator) animator = GetComponentInChildren<Animator>();
            SetRespawn(transform.position, transform.rotation, null);
        }

        public void SetRespawn(Vector3 worldPos, Quaternion rot, Transform anchor)
        {
            respawnAnchor = anchor; respawnRot = rot;
            respawnPos = anchor ? anchor.InverseTransformPoint(worldPos) : worldPos;
        }

        public void Respawn()
        {
            var p = respawnAnchor ? respawnAnchor.TransformPoint(respawnPos) : respawnPos;
            Teleport(p, respawnRot);
        }

        public void Teleport(Vector3 p, Quaternion r)
        {
            cc.enabled = false;
            transform.SetPositionAndRotation(p, r);
            cc.enabled = true;
            velocity = Vector3.zero; platform = null;
        }

        void Update()
        {
            float dt = Time.deltaTime; if (dt <= 0) return;
            Physics.SyncTransforms();

            if (platform)
            {
                Vector3 target = platform.TransformPoint(platformLocal);
                Vector3 d = target - transform.position;
                if (d.sqrMagnitude < 4f) cc.Move(d);
                float dy = Mathf.DeltaAngle(0, (platform.rotation * Quaternion.Inverse(platformRot)).eulerAngles.y);
                transform.Rotate(0, dy, 0, Space.World);
            }

            bool locked = GameState.InputLocked || GameState.UIBlocking;
            Vector2 mv = Vector2.zero; bool sprint = false;
            var kb = Keyboard.current;
            if (kb != null && !locked)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) mv.y += 1;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) mv.y -= 1;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) mv.x += 1;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) mv.x -= 1;
                sprint = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
                if (kb.spaceKey.wasPressedThisFrame) lastJump = Time.time;
            }

            var cam = Camera.main ? Camera.main.transform : null;
            Vector3 fwd = cam ? Vector3.ProjectOnPlane(cam.forward, Vector3.up) : transform.forward;
            if (fwd.sqrMagnitude < 0.001f) fwd = transform.forward;
            fwd.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, fwd);
            Vector3 move = fwd * mv.y + right * mv.x;
            if (move.sqrMagnitude > 1) move.Normalize();
            Vector3 horiz = move * (sprint ? sprintSpeed : walkSpeed);
            if (move.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 1 - Mathf.Exp(-turnSpeed * dt));

            Grounded = cc.isGrounded || GroundCheck(out _);
            if (Grounded) lastGrounded = Time.time;
            if (Grounded && velocity.y < 0) velocity.y = -2f;
            if (Time.time - lastJump < 0.15f && Time.time - lastGrounded < 0.15f)
            {
                velocity.y = Mathf.Sqrt(2 * jumpHeight * -gravity);
                lastJump = -10; lastGrounded = -10; platform = null; Grounded = false;
                if (AudioManager.I) AudioManager.I.Play(Sfx.Whoosh, 0.12f);
            }
            velocity.y = Mathf.Max(velocity.y + gravity * dt, -45f);
            cc.Move((horiz + Vector3.up * velocity.y) * dt);

            if (velocity.y <= 0 && GroundCheck(out var hit))
            {
                var cp = hit.collider.GetComponentInParent<CarryPlatform>();
                if (cp) { platform = cp.transform; platformLocal = platform.InverseTransformPoint(transform.position); platformRot = platform.rotation; }
                else platform = null;
            }
            else platform = null;

            if (animator)
            {
                animator.SetFloat("Speed", horiz.magnitude, 0.1f, dt);
                animator.SetBool("Grounded", Grounded);
            }
            if (transform.position.y < killY) Respawn();
        }

        bool GroundCheck(out RaycastHit hit)
        {
            float r = cc.radius * 0.9f;
            Vector3 origin = transform.position + Vector3.up * (r + 0.1f);
            return Physics.SphereCast(origin, r, Vector3.down, out hit, 0.3f, Mask, QueryTriggerInteraction.Ignore);
        }
    }
}
