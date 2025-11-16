using UnityEngine;

namespace Weaponry
{
    /// <summary>
    /// Interface que define o efeito da arma.
    /// Igualmente utilizada para execução do ataque.
    /// </summary>
    public interface IWeaponEffect
    {
        public void Execute(WeaponData data, Vector3 origin, Quaternion direction);
    }
}