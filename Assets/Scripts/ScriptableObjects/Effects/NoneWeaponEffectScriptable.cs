using UnityEngine;
using Weaponry;
using Weaponry.Effect;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "None effect", menuName = "Weapons/None Weapon Effect")]
    public class NoneWeaponEffectScriptable : BaseWeaponEffectScriptable
    {
        [SerializeField] private NoneEffect noneEffect;
        public override IWeaponEffect GetWeaponEffect()
        {
            return noneEffect ??= new NoneEffect();
        }
    }
}