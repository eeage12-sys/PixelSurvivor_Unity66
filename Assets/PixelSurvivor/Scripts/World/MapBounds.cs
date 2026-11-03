using UnityEngine;
namespace PixelSurvivor
{
    public sealed class MapBounds : MonoBehaviour
    {
        [SerializeField] Vector2 halfSize = new Vector2(32, 20);
        public Vector2 HalfSize => halfSize;
        public Vector2 Clamp(Vector2 position, float margin = 0)
        {
            Vector2 center = transform.position;
            float x = Mathf.Max(0, halfSize.x - margin), y = Mathf.Max(0, halfSize.y - margin);
            return new Vector2(Mathf.Clamp(position.x, center.x - x, center.x + x),
                Mathf.Clamp(position.y, center.y - y, center.y + y));
        }
        void OnDrawGizmosSelected() { Gizmos.color = Color.cyan; Gizmos.DrawWireCube(transform.position, halfSize * 2); }
    }
}
