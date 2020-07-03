using Project.Networking;
using Project.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Player
{
    public class PlayerManager : MonoBehaviour
    {
        public CharacterController controller;
        [SerializeField]
        private Transform cam;

        [Header("Data")]
        [SerializeField]
        private float speed = 6;
        [SerializeField]
        private float turnSmoothTime = 0.1f;
        private float turnSmoothVelocity; //Ref

        [Header("Class Reference")]
        [SerializeField]
        private NetworkIdentity networkIdentity;

        private float lastRotation;


        //Attack
        private Cooldown attackCooldown;

        public void Start()
        {
            cam = Camera.main.transform;
            attackCooldown = new Cooldown(1);
          

        }
        public void Update()
        {

            if (networkIdentity.IsControlling()) {
                CheckMovement();
            // CheckAiming();
            // CheckShooting();
            }
        }

        public float GetLastRotation()
        {
            return lastRotation;
        }

        public void SetRotation(float value)
        {
            transform.rotation = Quaternion.Euler(0, 0, value);
        }

        void CheckMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            if(direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f)*Vector3.forward;
                controller.Move(moveDir.normalized * speed * Time.deltaTime);
            }


        }

        void CheckAiming()
        {
           
        }

        void CheckShooting()
        {
            attackCooldown.CooldownUpdate();

            if (Input.GetMouseButton(0) && !attackCooldown.IsOnCooldown())
            {
                attackCooldown.StartCooldown();

                //Define Bullet
               

                //Send Attack
                //networkIdentity.GetSocket().Emit("attack", new JSONObject(JsonUtility.ToJson(bulletData)));

            }
        }
    }
}

