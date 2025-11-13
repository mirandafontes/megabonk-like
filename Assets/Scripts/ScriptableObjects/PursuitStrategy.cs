using Pursuit;
using UnityEngine;

namespace ScriptableObjects
{
    public abstract class PursuitStrategy : ScriptableObject
    {
        public abstract IPursuit GetBehavior();
    }
}