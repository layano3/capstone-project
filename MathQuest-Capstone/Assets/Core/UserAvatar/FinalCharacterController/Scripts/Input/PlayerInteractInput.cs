using UnityEngine;
using UnityEngine.InputSystem;

namespace UserAvatar.FinalCharacterController
{
    [DefaultExecutionOrder(-2)]
    public class PlayerInteractInput : MonoBehaviour, PlayerControls.IPlayerActionMapActions
    {
        public bool AttackPressed { get; private set; }
        public bool GatherPressed { get; private set; }

        private PlayerLocomotionInput _playerLocomotionInput;
        [SerializeField] private Interactor _interactor;

        private PlayerControls _localControls; // fallback if singleton missing
        private PlayerControls Controls =>
            PlayerInputManager.Instance?.PlayerControls ?? _localControls;

        private void Awake()
        {
            _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
            if (_interactor == null) _interactor = GetComponentInChildren<Interactor>() ?? GetComponent<Interactor>();

            if (PlayerInputManager.Instance == null)
                _localControls = new PlayerControls();
        }

        private void OnEnable()
        {
            if (Controls == null) return;
            Controls.PlayerActionMap.Enable();
            Controls.PlayerActionMap.SetCallbacks(this);
        }

        private void OnDisable()
        {
            if (Controls == null) return;
            Controls.PlayerActionMap.SetCallbacks(null);
            Controls.PlayerActionMap.Disable();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                _interactor?.TryInteract();

            if (_playerLocomotionInput != null && _playerLocomotionInput.MovementInput != Vector2.zero)
                GatherPressed = false;
        }

        public void OnInteract(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                _interactor?.TryInteract();
        }

        public void OnAttack(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                AttackPressed = true;
        }

        public void OnGather(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                GatherPressed = true;
        }
    }
}
