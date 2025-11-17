using UnityEngine;

namespace Pursuit
{
    [System.Serializable]
    public class SimplePursuit : IPursuit
    {
        [Header("- Pursuit -")]
        [SerializeField] private string playerTag;
        [SerializeField] private string enemyTag = "Enemy";
        [SerializeField] private float avoidanceCheckDistance = 1.0f;
        [SerializeField] private LayerMask obstacleAvoidanceMask;

        [Header("- Avoid -")]
        [SerializeField] private float separationRadius = 0.2f;
        [SerializeField] private float separationWeight = 0.25f;
        [SerializeField] private float minSeparationDistance = 0.1f;
        [SerializeField] private float minStopDistance = 1f;
        [SerializeField] private float directionLerpSpeed = 10f;
        private Vector3 lastDirection = Vector3.forward;

        private readonly int maxSeparationCount = 5;
        private static readonly Collider[] separationResults = new Collider[5];

        public MovementResult CalculateMovement(Vector3 currentPosition, Vector3 targetPosition, float deltaTime)
        {
            float stopDistanceSqr = minStopDistance * minStopDistance;
            Vector3 toTarget = targetPosition - currentPosition;
            
            //Verificar se o inimigo está muito próximo do player
            //para interromper a perseguição na posição atual
            if (toTarget.sqrMagnitude < stopDistanceSqr)
            {
                return new MovementResult
                {
                    FinalDirection = Vector3.zero, 
                    IsAvoidingObstacle = false
                };
            }

            Vector3 targetDirection = toTarget.normalized;
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
                deltaTime * directionLerpSpeed
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

            //maxSeparationCount atua como limitante, para evitar comparações infinitas
            for (int i = 0; i < numHits && i < maxSeparationCount; i++)
            {
                Collider other = separationResults[i];
                Vector3 otherPosition = other.transform.position;
                if (otherPosition == currentPosition || !other.CompareTag(enemyTag))
                {
                    continue;
                }

                Vector3 awayFromNeighbor = currentPosition - otherPosition;
                float sqrDistance = awayFromNeighbor.sqrMagnitude; 

                if (sqrDistance > 0.0001f)
                {
                    float distance = Mathf.Sqrt(sqrDistance);
                    float clampedDistance = Mathf.Max(minSeparationDistance, distance);

                    // weight = (vetor normalizado) / distância_com_peso
                    // weight = (awayFromNeighbor / distance) / clampedDistance
                    // weight = awayFromNeighbor / (distance * clampedDistance)
                    separationVector += awayFromNeighbor / (distance * clampedDistance);
                }
            }

            return separationVector.normalized;
        }
    }
}
