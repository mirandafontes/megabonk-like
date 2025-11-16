using UnityEngine;
using Weaponry;

namespace ScriptableObjects
{
    public abstract class BaseWeaponEffectScriptable : ScriptableObject
    {
        protected IWeaponEffect weaponEffect;
        public abstract IWeaponEffect GetWeaponEffect();
    }
}