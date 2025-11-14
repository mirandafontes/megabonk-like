using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("- Movement -")]
        [SerializeField]
        private MovementSettings currentMovementScriptable;
        [SerializeField]
        private PlayerInput playerInput;

        [Header("- Components -")]
        [SerializeField]
        private Rigidbody rb;

        private Vector2 movementInput;
        private Vector3 currentMovementVector;
        private Vector3 flatVelocity;
        private Vector3 oppositeVelocity;
        private Vector3 tempMovementVector;

        #region Unity
        private void Awake()
        {
            SetupRigidbody();
            BindInputs();
        }

        private void FixedUpdate()
        {
            tempMovementVector.x = movementInput.x;
            tempMovementVector.y = 0f;
            tempMovementVector.z = movementInput.y;

            currentMovementVector = tempMovementVector.normalized;

            if (movementInput.magnitude > 0.1f)
            {
                rb.AddForce(currentMovementVector * currentMovementScriptable.AccelerationForce, ForceMode.Acceleration);
                LimitVelocity();
            }
            else
            {
                oppositeVelocity = rb.linearVelocity;
                oppositeVelocity.x *= -1f;
                oppositeVelocity.z *= -1f;
                oppositeVelocity.y = 0f;

                rb.AddForce(oppositeVelocity * currentMovementScriptable.InertiaDamping, ForceMode.Acceleration);
            }
        }
        #endregion

        private void BindInputs()
        {
            InputAction moveAction = playerInput.actions.FindAction("Move");

            if (moveAction == null)
            {
                Debug.LogError("[PlayerMovement] = MoveAction is null. Review player actions scriptable.");
                return;
            }

            moveAction.performed += OnMove;
            moveAction.canceled += OnMoveCanceled;
        }

        private void SetupRigidbody()
        {
            rb.freezeRotation = true;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            movementInput = Vector2.zero;
        }

        private void LimitVelocity()
        {
            flatVelocity.x = rb.linearVelocity.x;
            flatVelocity.y = 0f;
            flatVelocity.z = rb.linearVelocity.z;

            float magnitude = flatVelocity.magnitude;

            if (magnitude > currentMovementScriptable.MaxSpeed)
            {
                float ratio = currentMovementScriptable.MaxSpeed / magnitude;

                flatVelocity.x *= ratio;
                flatVelocity.z *= ratio;

                rb.linearVelocity = new Vector3(flatVelocity.x, rb.linearVelocity.y, flatVelocity.z);
            }
        }
    }
}
