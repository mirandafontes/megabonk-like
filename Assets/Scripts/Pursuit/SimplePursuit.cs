using UnityEngine;

namespace Pursuit
{
    [System.Serializable]
    public class SimplePursuit : IPursuit
    {
        [Header("- Pursuit -")]
        [SerializeField] private string playerTag;
        [SerializeField] private float avoidanceCheckDistance = 1.0f;
        [SerializeField] private LayerMask obstacleAvoidanceMask;

        [Header("- Avoid -")]
        [SerializeField] private float separationRadius = 0.2f;
        [SerializeField] private float separationWeight = 0.25f;
        [SerializeField] private float minSeparationDistance = 0.1f;
        [SerializeField] private float directionLerpSpeed = 10f; 
        private Vector3 lastDirection = Vector3.forward;

        private static readonly Collider[] separationResults = new Collider[5];

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

            //Se já não estiver evitando
            //verificamos se devemos evitar os inimigos
            if (!isAvoiding)
            {
                Vector3 separationForce = CalculateSeparation(currentPosition);
                if (separationForce.sqrMagnitude > 0)
                {
                    Vector3 combinedForce = finalDirection + separationForce * separationWeight;
                    finalDirection = combinedForce.normalized;

                    isAvoiding = true;
                }
            }

            Vector3 targetCombinedDirection = finalDirection.normalized;

            finalDirection = Vector3.Lerp(
                lastDirection, 
                targetCombinedDirection, 
                Time.deltaTime * directionLerpSpeed
            ).normalized; 

            lastDirection = finalDirection;

            return new MovementResult
            {
                FinalDirection = finalDirection,
                IsAvoidingObstacle = isAvoiding
            };
        }

        private Vector3 CalculateSeparation(Vector3 currentPosition)
        {
            Vector3 separationVector = Vector3.zero;

            int numHits = Physics.OverlapSphereNonAlloc(
                currentPosition,
                separationRadius,
                separationResults,
                obstacleAvoidanceMask
            );

            for (int i = 0; i < numHits && i < separationResults.Length; i++)
            {
                Collider other = separationResults[i];

                if (other.transform.position == currentPosition || !other.CompareTag("Enemy"))
                {
                    separationResults[i] = null;
                    continue;
                }

                Vector3 awayFromNeighbor = currentPosition - other.transform.position;
                float distance = awayFromNeighbor.magnitude;

                if (distance > 0)
                {
                    float clampedDistance = Mathf.Max(minSeparationDistance, distance);
                    separationVector += awayFromNeighbor.normalized / clampedDistance;
                }

                separationResults[i] = null;
            }

            return separationVector.normalized;
        }
    }
}
