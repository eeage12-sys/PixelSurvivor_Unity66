using UnityEngine;
namespace PixelSurvivor
{
    public sealed class GameTimer : MonoBehaviour
    {
        public float Elapsed { get; private set; }
        public void ResetTimer() => Elapsed = 0;
        public void AdvanceCampaign(float delta) => Elapsed += Mathf.Max(0, delta);
        // Compatibility for external callers: advancing time can never win a run.
        public bool Tick(float delta) { AdvanceCampaign(delta); return false; }
    }
}
