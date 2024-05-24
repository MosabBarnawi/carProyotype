using UnityEngine;

namespace BarnoGames.QuadTree.Sample
{
    public class LookableObject : MonoBehaviour, ISpacialData2D
    {
        [SerializeField] private Collider LinkedCollider;

        Vector3 CachedPosition;
        Rect? CachedBounds;
        Vector2? Cached2DPosition;
        float? CachedRadius;

        bool HasMoved
        {
            get
            {
                return !Mathf.Approximately((transform.position - CachedPosition).sqrMagnitude, 0f);
            }
        }

        public Vector2 GetLocation()
        {
            if (Cached2DPosition == null)
                CachePositionData();

            return Cached2DPosition.Value;
        }

        public Rect GetBounds()
        {
            if (CachedBounds == null)
                CachePositionData();

            return CachedBounds.Value;
        }

        public float GetRadius()
        {
            if (CachedRadius == null)
                CachePositionData();

            return CachedRadius.Value;
        }

        private void CachePositionData()
        {
            CachedPosition = transform.position;
            Cached2DPosition = new Vector2(CachedPosition.x, CachedPosition.z);
            float halfWidth = LinkedCollider.bounds.size.x;
            float halfHeight = LinkedCollider.bounds.size.z;

            CachedBounds = new Rect(CachedPosition.x - halfWidth,
                                    CachedPosition.z - halfHeight,
                                    halfWidth,
                                    halfHeight);

            CachedRadius = Mathf.Sqrt((halfWidth * halfWidth) + (halfHeight * halfHeight));
        }

        public void OnVisable()
        {
            gameObject.SetActive(true);
        }

        public void OnInvisable()
        {
            gameObject.SetActive(false);
        }
    }
}
