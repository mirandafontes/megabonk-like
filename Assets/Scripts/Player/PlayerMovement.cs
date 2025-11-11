using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        private MovementSettings currentMovementScriptable;
        [SerializeField]
        private PlayerInput playerInput;

        [Header("Components")]
        [SerializeField]
        private Rigidbody rb;

        private Vector2 movementInput;
        private Vector3 currentMovementVector;

        #region Unity
        private void Awake()
        {
            SetupRigidbody();
            BindInputs();
        }

        private void FixedUpdate()
        {
            currentMovementVector = new Vector3(movementInput.x, 0f, movementInput.y).normalized;
            if (movementInput.magnitude > 0.1f)
            {
                rb.AddForce(currentMovementVector * currentMovementScriptable.AccelerationForce, ForceMode.Acceleration);
                LimitVelocity();
            }
            else
            {
                Vector3 oppositeVelocity = -rb.linearVelocity;
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
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVelocity.magnitude > currentMovementScriptable.MaxSpeed)
            {
                Vector3 limitedVelocity = flatVelocity.normalized * currentMovementScriptable.MaxSpeed;

                rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }
        }
    }
}
