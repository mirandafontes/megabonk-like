using UnityEngine;

namespace ScriptableObjects
{
    //Mantendo a lógica presente no Enemy.
    //Blueprint representa o scriptableObject.
    //WeaponData são os dados atuais da arma.
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon Blueprint")]
    public class WeaponBlueprint : ScriptableObject
    {
        [Header("- Identity -")]
        public string WeaponName = "New Weapon";
        [TextArea] public string Description = "A basic weapon.";
        public Sprite Icon;

        [Header("- Base Stats -")]
        public float BaseDamage = 10f;
        public float BaseCooldown = 1.5f;
        public int BaseAmount = 1;
        public int MaxLevel = 5;
        public bool AutoAttack = false;

        [Header("- Evolution Stats -")]
        [Tooltip("Eixo X: Nível (0 a MaxLevel). Eixo Y: Dano Total no Nível X.")]
        public AnimationCurve DamageCurve;

        [Tooltip("Eixo X: Nível. Eixo Y: Cooldown")]
        public AnimationCurve CooldownCurve;

        [Tooltip("Eixo X: Nível. Eixo Y: Quantidade de Projéteis/Efeitos")]
        public AnimationCurve AmountCurve;

        public float GetValueAtLevel(AnimationCurve curve, int level)
        {
            float levelIndex = Mathf.Clamp(level, 0, MaxLevel);
            return curve.Evaluate(levelIndex);
        }

        [Header("- Effect -")]
        public BaseWeaponEffectScriptable WeaponEffect;
        public BaseWeaponEffectScriptable WeaponAttack;
    }
}
