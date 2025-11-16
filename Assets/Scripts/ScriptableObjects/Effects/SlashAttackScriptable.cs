using UnityEngine;
using Weaponry;
using Weaponry.Effect;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Slash attack", menuName = "Weapons/Slash Attack")]
    public class SlashAttackScriptable : BaseWeaponEffectScriptable
    {
        [SerializeField] private SlashAttack splashAttack;
        public override IWeaponEffect GetWeaponEffect()
        {
            return splashAttack ??= new SlashAttack();
        }
    }
}