using System.Collections;
using System.Collections.Generic;
using BarnoGames.QuadTree;
using UnityEngine;

namespace BarnoGames.LevelChunkLoading
{
    public class PlayerScript : MonoBehaviour
    {
        [SerializeField] QuadTree.QuadTree LinkedQuadTree;
        [SerializeField] float ObstacleSearchRange = 30f;


        Vector3 CachedPosition;
        Vector2? Cached2DPosition;

        HashSet<ISpacialData2D> NearbyObstacles;

        bool HasMoved
        {
            get
            {
                return !Mathf.Approximately((transform.position - CachedPosition).sqrMagnitude, 0f);
            }
        }


        #region UNITY METHODS

        private void Start()
        {
            LinkedQuadTree = FindFirstObjectByType<QuadTree.QuadTree>(findObjectsInactive: FindObjectsInactive.Exclude);
            Debug.Assert(LinkedQuadTree != null, "LinkedQuadTree is Null");
            
        }

        private void Update()
        {
            if (Cached2DPosition == null || HasMoved)
            {
                CachedPosition = transform.position;
                Cached2DPosition = new Vector2(CachedPosition.x, CachedPosition.z);

                HighlightNearbyObjects();
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            if (LinkedQuadTree != null)
                LinkedQuadTree.DrawWireFrame();
        }

        #endregion

        public void OnSpawnCompleted_TEMP()
        {
            HighlightNearbyObjects();
        }

        private void HighlightNearbyObjects()
        {
            if (LinkedQuadTree == null)
                return;

            HashSet<ISpacialData2D> candidateObstacles = LinkedQuadTree.FindDataInRange(Cached2DPosition.Value, ObstacleSearchRange);

            // Identify Removeals
            if (NearbyObstacles != null)
            {
                foreach (var oldObstacle in NearbyObstacles)
                {
                    if (candidateObstacles.Contains(oldObstacle))
                        continue;

                    ProcessRemoveObstacles(oldObstacle);
                }
            }


            if (candidateObstacles == null)
                return;


            // FIRST TIME FINIDING OBSTACLES ?
            if (NearbyObstacles == null)
            {
                NearbyObstacles = candidateObstacles;

                foreach (var newObstacle in NearbyObstacles)
                    ProcessAddObstacle(newObstacle);

                return;
            }

            // IDNETIFY ADDITIONS
            foreach (var newObstacle in candidateObstacles)
            {
                if (NearbyObstacles.Contains(newObstacle))
                    continue;

                ProcessAddObstacle(newObstacle);
            }

            NearbyObstacles = candidateObstacles;
        }

        private void ProcessAddObstacle(ISpacialData2D newObstacle)
        {
            newObstacle.OnVisable();
        }

        private void ProcessRemoveObstacles(ISpacialData2D removedObstacle)
        {
            removedObstacle.OnInvisable();
        }
    }
}
