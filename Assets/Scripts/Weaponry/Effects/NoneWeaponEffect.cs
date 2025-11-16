using UnityEngine;

namespace Weaponry.Effect
{
    public class NoneEffect : IWeaponEffect
    {
        public void Execute(WeaponData data, Vector3 origin, Quaternion direction)
        {
            return;
        }
    }
}