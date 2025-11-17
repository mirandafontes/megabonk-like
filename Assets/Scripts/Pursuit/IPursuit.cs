
using UnityEngine;

namespace Pursuit
{
    public interface IPursuit
    {
        public MovementResult CalculateMovement(Vector3 currentPosition, Vector3 targetPosition, float deltaTime);
    }
}
