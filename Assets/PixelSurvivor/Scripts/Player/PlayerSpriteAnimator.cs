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
        public bool IsConfigured => Valid(downFrames) && Valid(upFrames) && Valid(leftFrames) && Valid(rightFrames);
        static bool Valid(Sprite[] frames)
        {
            if (frames == null || frames.Length != 4) return false;
            foreach (Sprite frame in frames) if (frame == null) return false;
            return true;
        }
        public void Configure(Sprite[] down, Sprite[] up, Sprite[] left, Sprite[] right)
        { downFrames = down; upFrames = up; leftFrames = left; rightFrames = right; ResetAnimation(); }
        public void ResetAnimation() { phase = 0; facing = 0; ShowFrame(0); }
        public void AnimateStep(Vector2 actualDisplacement)
        {
            if (actualDisplacement.sqrMagnitude < 0.00000001f)
            { phase = 0; ShowFrame(0); return; }
            int next = Mathf.Abs(actualDisplacement.x) > Mathf.Abs(actualDisplacement.y)
                ? (actualDisplacement.x < 0 ? 2 : 3) : (actualDisplacement.y > 0 ? 1 : 0);
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
