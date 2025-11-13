using Pursuit;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Simple pursuit", menuName = "Data/Simple Pursuit")]
    public class SimplePursuitStrategy : PursuitStrategy
    {
        [SerializeField] private SimplePursuit pursuit;
        public override IPursuit GetBehavior()
        {
            return pursuit ??= new SimplePursuit();
        }
    }
}