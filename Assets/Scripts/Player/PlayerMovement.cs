using ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using Weaponry;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("- Dependencies -")]
        //Para debug
        [SerializeField] private PlayerStats playerStats;
        [SerializeField] private PlayerInput playerInput;

        [Header("- Weapons -")]
        [SerializeField] private PlayerWeapon playerWeapon;
        [SerializeField] private WeaponBlueprint swordBlueprint;
        [SerializeField] private WeaponBlueprint bowBlueprint;

        [Header("- Components -")]
        [SerializeField] private Rigidbody rb;

        private Vector2 movementInput;
        private Vector3 currentMovementVector;
        private Vector3 flatVelocity;
        private Vector3 oppositeVelocity;
        private Vector3 tempMovementVector;

        #region Unity
        private void Awake()
        {
            EquipeWeapons();
            SetupRigidbody();
            BindInputs();
        }

        private void FixedUpdate()
        {
            if (playerStats == null)
            {
                return;
            }

            tempMovementVector.x = movementInput.x;
            tempMovementVector.y = 0f;
            tempMovementVector.z = movementInput.y;

            currentMovementVector = tempMovementVector.normalized;

            if (movementInput.magnitude > 0.1f)
            {
                rb.AddForce(currentMovementVector * playerStats.AccelerationForce, ForceMode.Acceleration);
                LimitVelocity();
            }
            else
            {
                oppositeVelocity = rb.linearVelocity;
                oppositeVelocity.x *= -1f;
                oppositeVelocity.z *= -1f;
                oppositeVelocity.y = 0f;

                rb.AddForce(oppositeVelocity * playerStats.InertiaDamping, ForceMode.Acceleration);
            }
        }
        #endregion

        public void Initialize(PlayerStats playerStats)
        {
            this.playerStats = playerStats;
        }

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

            if (magnitude > playerStats.MaxSpeed)
            {
                float ratio = playerStats.MaxSpeed / magnitude;

                flatVelocity.x *= ratio;
                flatVelocity.z *= ratio;

                rb.linearVelocity = new Vector3(flatVelocity.x, rb.linearVelocity.y, flatVelocity.z);
            }
        }

        private void EquipeWeapons()
        {
            //Infelizmente, as armas estão de maneira estática
            //Gostaria que fosse de outra maneira, mas estou sem tempo ;3;
            WeaponData sword = new WeaponData(swordBlueprint);
            WeaponData bow = new WeaponData(bowBlueprint);

            playerWeapon.EquipWeapon(sword);
            playerWeapon.EquipWeapon(bow);
        }
    }
}
