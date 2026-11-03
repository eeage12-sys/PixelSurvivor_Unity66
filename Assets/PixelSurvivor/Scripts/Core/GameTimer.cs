using UnityEngine;
namespace PixelSurvivor
{
    public sealed class GameTimer : MonoBehaviour
    {
        [SerializeField, Min(1)] float roundDuration = 300;
        public float Elapsed { get; private set; }
        public float Duration => roundDuration;
        public float Remaining => Mathf.Max(0, Duration - Elapsed);
        public void ResetTimer() => Elapsed = 0;
        public bool Tick(float delta)
        { Elapsed = Mathf.Min(Duration, Elapsed + Mathf.Max(0, delta)); return Elapsed >= Duration; }
    }
}
