using Player;

namespace Weaponry
{
    public interface IItemModifier
    {
        public float ModifierValue { get; }
        public void Initialize(PlayerStats stats);
        public void Apply();
        public void Remove();
    }
}