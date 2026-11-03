using UnityEngine;
namespace PixelSurvivor
{
    // Pure rules shared by gameplay and EditMode tests.
    public static class GameRules
    {
        public const float MinimumAttackInterval = 0.12f;
        public static int RequiredExperience(int level) => 5 + (Mathf.Max(1, level) - 1) * 3;
        public static float SpawnInterval(float elapsed, float[] stages)
        {
            if (stages == null || stages.Length == 0) return 1.5f;
            return Mathf.Max(0.05f, stages[Mathf.Clamp(Mathf.FloorToInt(elapsed / 60f), 0, stages.Length - 1)]);
        }
        public static Vector2 Movement(Vector2 input) => Vector2.ClampMagnitude(input, 1f);
        public static string Clock(float seconds)
        {
            int total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return $"{total / 60:00}:{total % 60:00}";
        }
        // Continuous circle collision: avoids skipping enemies with fast projectiles/low FPS.
        public static bool SegmentCircle(Vector2 start, Vector2 end, Vector2 center, float radius, out float fraction)
        {
            Vector2 delta = end - start;
            Vector2 offset = start - center;
            fraction = 0;
            float c = Vector2.Dot(offset, offset) - radius * radius;
            if (c <= 0) return true;
            float a = Vector2.Dot(delta, delta);
            if (a < 0.000001f) return false;
            float b = Vector2.Dot(offset, delta);
            float discriminant = b * b - a * c;
            if (discriminant < 0) return false;
            fraction = (-b - Mathf.Sqrt(discriminant)) / a;
            return fraction >= 0 && fraction <= 1;
        }
    }
}
