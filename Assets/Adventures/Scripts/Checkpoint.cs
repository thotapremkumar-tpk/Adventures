using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Adventures
{
    public class Checkpoint : PlayerZone
    {
        public Transform spawnPoint;
        public Transform anchor;
        public bool announce = true;
        bool reached;
        protected override void OnPlayerEnter(PlayerController p)
        {
            var sp = spawnPoint ? spawnPoint : transform;
            p.SetRespawn(sp.position, sp.rotation, anchor);
            if (!reached && announce) UIManager.I.Toast("Checkpoint reached", 2f);
            reached = true;
        }
    }
}
