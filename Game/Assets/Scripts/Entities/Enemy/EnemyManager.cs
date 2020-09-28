using Project.Networking;
using Project.Utility;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Entities.Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        [Header("Agent Settings")]
        public NavMeshPath path;
        NavMeshAgent agent;
        private Coroutine rotationCoroutine;

        [Header("Stats Settings")]
        public int maxHealth;
        public int currentHealth;
        public int damage;
        public float attackSpeed;
        Cooldown cooldownBetweenAttack;
        [Header("Vision Settings")]

        public FieldOfView fov;
        public float stopRange = 1;

        [Header("MultiplayerData")]
        public NetworkIdentity networkIdentity;
        

        [Header("Components")]
        private Animator animator;
        void Start()
        {
            path = new NavMeshPath();
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
           // this.state = EnemyState.IDLE;
            this.cooldownBetweenAttack = new Cooldown(this.attackSpeed);
            this.currentHealth = maxHealth;
        }

        void Update()
        {
        }

        public void Die()
        {
            //Animation
            this.animator.SetTrigger("Death");
            Debug.Log("Zombie Die");
            
        }

        public void SetRotation(float value)
        {
            rotationCoroutine = StartCoroutine(AnimateTankTurn(transform.localEulerAngles.z, value));
        }

        public void StopCoroutines()
        {
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
            }
        }

        private IEnumerator AnimateTankTurn(float startRotation, float goalRotation)
        {
            float count = 0.1f; //In sync with server update
            float currentTime = 0.0f;

            while (currentTime < count)
            {
                currentTime += Time.deltaTime;

                if (currentTime < count)
                {
                    transform.localEulerAngles = new Vector3(0, 0,
                        Mathf.LerpAngle(startRotation, goalRotation, currentTime / count));
                }

                yield return new WaitForEndOfFrame();

                if (transform == null)
                {
                    currentTime = count;
                    yield return null;
                }
            }

            yield return null;
        }

        private IEnumerator AIPositionSmoothing(Transform aiTransform, Vector3 goalPosition)
        {
            float count = 0.1f; //In sync with server update
            float currentTime = 0.0f;
            Vector3 startPosition = aiTransform.position;

            while (currentTime < count)
            {
                currentTime += Time.deltaTime;

                if (currentTime < count)
                {
                    aiTransform.position = Vector3.Lerp(startPosition, goalPosition, currentTime / count);
                }

                yield return new WaitForEndOfFrame();

                if (aiTransform == null)
                {
                    currentTime = count;
                    yield return null;
                }
            }

            yield return null;
        }
    }
}

