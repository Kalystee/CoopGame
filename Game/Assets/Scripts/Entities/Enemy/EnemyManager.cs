using Project.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Entities.Enemy
{
    public class EnemyManager : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        public int maxHealth = 20;
        [SerializeField]
        private int currentHealth;
        
        [Header("MultiplayerData")]
        public NetworkIdentity networkIdentity;

        void Start()
        {
            currentHealth = maxHealth;
        }

        void Update()
        {

        }

        public void Die()
        {
            //Animation
            Destroy(this.gameObject);
        }

        public bool isDead()
        {
            return this.currentHealth <= 0;
        }

        public void TakeDamage(int ammount)
        {
            this.currentHealth -= ammount;
        }
    }
}

