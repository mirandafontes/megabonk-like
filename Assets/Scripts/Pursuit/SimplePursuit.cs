using UnityEngine;

namespace Pursuit
{
    [System.Serializable]
    public class SimplePursuit : IPursuit
    {
        [SerializeField] private string playerTag;
        [SerializeField] private float avoidanceCheckDistance = 1.0f;
        [SerializeField] private LayerMask obstacleAvoidanceMask;

        public MovementResult CalculateMovement(Vector3 currentPosition, Vector3 targetPosition)
        {
            Vector3 targetDirection = (targetPosition - currentPosition).normalized;
            Vector3 finalDirection = targetDirection;
            bool isAvoiding = false;

            if (Physics.Raycast(currentPosition, targetDirection, out RaycastHit hit, avoidanceCheckDistance, obstacleAvoidanceMask))
            {
                if (!hit.collider.CompareTag(playerTag) && !hit.collider.isTrigger)
                {
                    Vector3 avoidanceVector = Vector3.Cross(hit.normal, Vector3.up);

                    if (Vector3.Dot(avoidanceVector, targetDirection) < Vector3.Dot(-avoidanceVector, targetDirection))
                    {
                        avoidanceVector = -avoidanceVector;
                    }

                    finalDirection = avoidanceVector.normalized;
                    isAvoiding = true;
                }
            }

            return new MovementResult
            {
                FinalDirection = finalDirection,
                IsAvoidingObstacle = isAvoiding
            };
        }
    }
}
