using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Entities
{
    public interface IDamageable
    {
        void TakeDamage(int ammount);
        bool isDead();
        void Die();
    }
}

