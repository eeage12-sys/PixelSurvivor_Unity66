using UnityEngine;
namespace PixelSurvivor
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] MapBounds bounds;
        Camera view;
        public void Configure(Transform follow, MapBounds map) { target = follow; bounds = map; }
        void Awake()
        {
            view = GetComponent<Camera>();
            // Keep the player and enemies legible at the actual Game view size.
            view.orthographicSize = 5.5f;
        }
        void LateUpdate() => Snap();
        public void Snap()
        {
            if (target == null || bounds == null) return;
            if (view == null) view = GetComponent<Camera>();
            float h = view.orthographicSize;
            float w = h * view.aspect;
            Vector2 center = bounds.transform.position;
            Vector2 limit = new Vector2(Mathf.Max(0, bounds.HalfSize.x - w), Mathf.Max(0, bounds.HalfSize.y - h));
            transform.position = new Vector3(Mathf.Clamp(target.position.x, center.x - limit.x, center.x + limit.x),
                Mathf.Clamp(target.position.y, center.y - limit.y, center.y + limit.y), -10);
        }
    }
}
