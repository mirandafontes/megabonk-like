using UnityEngine;
using Weaponry;

namespace ScriptableObjects
{
    public abstract class ItemBlueprint : ScriptableObject
    {
        [Header("- Visual -")]
        public Sprite ItemIcon;
        public string ItemName;
        public string ItemDescription;
        public ModifierType ItemType;

        public abstract IItemModifier GetItemModifier();


        public enum ModifierType
        {
            Speed,
            Health,
        }
    }
}