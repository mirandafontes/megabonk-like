using UnityEngine;
using Damageable;

namespace Weaponry.Effect
{
    [System.Serializable]
    public class SlashAttack : IWeaponEffect
    {
        private static readonly Collider[] hitResults = new Collider[50];
        public float Range = 1f;
        public float AttackAngle = 45f;
        public LayerMask EnemyLayer;

        public void Execute(WeaponData data, Vector3 origin, Quaternion direction)
        {
            int numHits = Physics.OverlapSphereNonAlloc(origin, Range, hitResults, EnemyLayer);
            Vector3 forward = direction * Vector3.forward;

            for (int i = 0; i < numHits && i < hitResults.Length && i < data.CurrentAmount; i++)
            {
                Collider hit = hitResults[i];

                if (hit == null)
                {
                    continue;
                }

                Vector3 targetDirection = (hit.transform.position - origin).normalized;
                float angleToTarget = Vector3.Angle(forward, targetDirection);

                if (angleToTarget <= AttackAngle / 2f)
                {
                    hit.GetComponent<IDamageable>()?.TakeDamage(data.CurrentDamage);
                }

                //Limpeza do array após o procedimento
                //Visando garantir a ativação correta do GC,
                //Uma vez que estamos em uma classe non-Unity
                //e utilizando componentes Unity;
                hitResults[i] = null;
            }
        }
    }
}