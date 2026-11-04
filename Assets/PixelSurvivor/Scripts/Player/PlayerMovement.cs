using UnityEngine;
using UnityEngine.InputSystem;
namespace PixelSurvivor
{
    [RequireComponent(typeof(PlayerStats))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] InputActionReference moveAction;
        [SerializeField, Min(0)] float radius = 0.36f;
        GameManager game;
        PlayerStats stats;
        PlayerSpriteAnimator spriteAnimator;
        public float Radius => radius;
        public InputActionReference MoveAction => moveAction;
        public void SetInput(InputActionReference action) => moveAction = action;
        public void Initialize(GameManager owner) { game = owner; stats = GetComponent<PlayerStats>(); spriteAnimator = GetComponent<PlayerSpriteAnimator>(); }
        void OnEnable() { if (moveAction != null) moveAction.action.Enable(); }
        void OnDisable() { if (moveAction != null) moveAction.action.Disable(); }
        void Update()
        {
            if (game == null || !game.IsPlaying || moveAction == null) return;
            Move(moveAction.action.ReadValue<Vector2>(), Time.deltaTime);
        }
        public void Move(Vector2 input, float dt)
        {
            if (game == null || !game.IsPlaying) return;
            Vector2 before = transform.position;
            Vector2 next = before + GameRules.Movement(input) * stats.MoveSpeed * Mathf.Max(0, dt);
            transform.position = game.Bounds.Clamp(next, radius);
            if (spriteAnimator != null) spriteAnimator.AnimateStep((Vector2)transform.position - before);
        }
    }
}
