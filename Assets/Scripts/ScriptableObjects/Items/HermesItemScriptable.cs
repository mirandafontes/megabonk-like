using UnityEngine;
using Weaponry.Items;
using Weaponry;

namespace ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "Hermes", menuName = "Item/Hermes Item")]
    public class HermesItemScriptable : ItemBlueprint
    {
        [SerializeField] private HermesSpeedModifier hermesSpeed;
        public override IItemModifier GetItemModifier()
        {
            return hermesSpeed ??= new HermesSpeedModifier();
        }
    }
}