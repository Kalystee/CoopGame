using Project.Entities.Enemy;
using Project.Networking;
using Project.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Entities.Player
{
    public class PlayerManager : MonoBehaviour
    {
        public CharacterController controller;
        private Animator animator;
        [SerializeField]
        private Transform cam;
        
        [Header("Data")]
        [SerializeField]
        private float speed = 6;
        [SerializeField]
        private int maxHealth = 10;
        [SerializeField]
        private int currentHealth;
        [SerializeField]
        private float turnSmoothTime = 0.1f;
        private float turnSmoothVelocity; //Ref
        private int attackDmg = 5;
        [Header("Class Reference")]
        [SerializeField]
        private NetworkIdentity networkIdentity;
        private float lastRotation;


        [Header("Attack")]
        private Cooldown attackCooldown;
        public Transform meleeAttackPoint;
        public float attackRange;
        public LayerMask ennemyLayers;


        private CombatData combatData;
        public void Start()
        {
            cam = Camera.main.transform;
            attackCooldown = new Cooldown(1);
            currentHealth = maxHealth;
            combatData = new CombatData();
            animator = GetComponent<Animator>();
        }
        public void Update()
        {

            if (networkIdentity.IsControlling()) {
                CheckMovement();
                CheckAttack();
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
                animator.SetFloat("Speed", direction.magnitude);
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f)*Vector3.forward;
                controller.Move(moveDir.normalized * speed * Time.deltaTime);
            }


        }


        void CheckAttack()
        {
            attackCooldown.CooldownUpdate();

            if (Input.GetMouseButton(0) && !attackCooldown.IsOnCooldown())
            {
                Debug.Log("Launch Attack");
                attackCooldown.StartCooldown();

                //Define Attack
                //Animation
               Collider[] hitEnemies = Physics.OverlapSphere(meleeAttackPoint.position, attackRange,ennemyLayers);
               foreach(Collider enemy in hitEnemies)
                {
                    Debug.Log("We hit " + enemy.name);
                    //The dmg are taken in the server
                    //Send Attack
                    combatData.initiatorId = this.networkIdentity.GetId();
                    combatData.targetId = enemy.GetComponent<NetworkIdentity>().GetId();
                    combatData.ammount = attackDmg.ToString();

                    networkIdentity.GetSocket().Emit("takeDamage", new JSONObject(JsonUtility.ToJson(combatData)));
                }


            }
        }

        public void Die()
        {
            //Disabling Collider?
            //Animation
            //Display UI
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(meleeAttackPoint.position, attackRange);
        }
    }
}

