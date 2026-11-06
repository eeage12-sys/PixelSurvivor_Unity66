using UnityEngine;
namespace PixelSurvivor
{
    // Discrete, replaceable PNG frames. Only SpriteRenderer.sprite is changed;
    // PlayerHealth retains ownership of the damage-flash color.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerSpriteAnimator : MonoBehaviour
    {
        [SerializeField] Sprite[] downFrames;
        [SerializeField] Sprite[] upFrames;
        [SerializeField] Sprite[] leftFrames;
        [SerializeField] Sprite[] rightFrames;
        [Tooltip("Animation frames per world unit travelled. Default: 10 fps at movement speed 5.")]
        [SerializeField, Min(0.1f)] float framesPerUnit = 2;
        SpriteRenderer visual;
        float phase;
        int facing; // 0 down, 1 up, 2 left, 3 right
        public int Facing => facing;
        public Vector2 AimDirection { get; private set; } = Vector2.down;
        public bool IsConfigured => Valid(downFrames) && Valid(upFrames) && Valid(leftFrames) && Valid(rightFrames);
        static bool Valid(Sprite[] frames)
        {
            if (frames == null || frames.Length != 4) return false;
            foreach (Sprite frame in frames) if (frame == null) return false;
            return true;
        }
        public void Configure(Sprite[] down, Sprite[] up, Sprite[] left, Sprite[] right)
        { downFrames = down; upFrames = up; leftFrames = left; rightFrames = right; ResetAnimation(); }
        public void ResetAnimation() { phase = 0; facing = 0; AimDirection = Vector2.down; ShowFrame(0); }
        public void AnimateStep(Vector2 actualDisplacement, Vector2 intendedDirection)
        {
            if (intendedDirection.sqrMagnitude > 0.0001f)
                AimDirection = intendedDirection.normalized;
            if (actualDisplacement.sqrMagnitude < 0.00000001f)
            { phase = 0; ShowFrame(0); return; }
            float horizontal = Mathf.Abs(intendedDirection.x), vertical = Mathf.Abs(intendedDirection.y);
            int horizontalFacing = intendedDirection.x < 0 ? 2 : 3;
            int verticalFacing = intendedDirection.y > 0 ? 1 : 0;
            // Equal diagonal axes can differ by a few floats from frame to frame.
            // Keep the current compatible facing instead of swapping sprites every frame.
            bool nearDiagonal = horizontal > .1f && vertical > .1f &&
                Mathf.Abs(horizontal - vertical) <= Mathf.Max(horizontal, vertical) * .15f;
            int next = nearDiagonal && (facing == horizontalFacing || facing == verticalFacing)
                ? facing : horizontal > vertical ? horizontalFacing : verticalFacing;
            if (next != facing) phase = 0;
            facing = next;
            phase = (phase + actualDisplacement.magnitude * framesPerUnit) % 4;
            ShowFrame(Mathf.FloorToInt(phase));
        }
        void ShowFrame(int index)
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            Sprite[] frames = facing == 1 ? upFrames : facing == 2 ? leftFrames : facing == 3 ? rightFrames : downFrames;
            if (visual != null && frames != null && frames.Length > 0 && frames[index % frames.Length] != null)
                visual.sprite = frames[index % frames.Length];
        }
    }
}
