using UnityEngine;

namespace Damageable
{
    public interface IDamageable
    {
        public void TakeDamage(float amount, GameObject instigator);
    }
}