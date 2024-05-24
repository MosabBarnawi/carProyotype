using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarnoGames.QuadTree;
using UnityEngine.UIElements;

namespace BarnoGames.LevelChunkLoading
{
    public class GroundSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject groundPrefab;
        [SerializeField, Min(1)] private Vector2Int bounds = new Vector2Int(5, 5);
        [SerializeField] private Transform Root;
        [SerializeField] private QuadTree.QuadTree quadTree;
        List<GameObject> groundTiles = new();

        Transform groundPrefabTransform;
        Vector3 BoundsSize;
        public Rect BoundsSizeRect;

        #region UNITY METHODS

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Color gizmoColor = Color.magenta;
            gizmoColor.a = 0.2f;

            Gizmos.color = gizmoColor;

            if (groundPrefab != null)
            {
                ReCalculateBounds();
                Vector3 pos = transform.position;
                pos.y = -0.3f;
                Gizmos.DrawCube(pos, BoundsSize);
            }

        }
#endif


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
                GenerateGround();
        }

        #endregion //UNITY METHODS

        private void ReCalculateBounds()
        {
            if (groundPrefab != null)
            {
                if (groundPrefabTransform == null)
                    groundPrefabTransform = groundPrefab.transform;

                Vector3 prefabSize = groundPrefabTransform.localScale;

                BoundsSize = new Vector3(prefabSize.x * bounds.x,
                   prefabSize.y,
                   prefabSize.z * bounds.y);

                BoundsSizeRect = new Rect(prefabSize.x * (-bounds.x / 2f),
                                        prefabSize.z * (-bounds.y / 2f),
                                        BoundsSize.x, BoundsSize.z);
            }

        }

        [ContextMenu("Spawn Gorund")]
        private void GenerateGround()
        {
            for (int i = 0; i < groundTiles.Count; i++)
            {
                GameObject item = groundTiles[i];
                Destroy(item);
            }
            groundTiles = new();

            quadTree.PrepareTree(BoundsSizeRect);

            Vector3 prefabSize = groundPrefabTransform.localScale;
            Transform parentRootTransform = Root != null ? Root : transform;

            for (int x = 0; x < bounds.x; x++)
            {
                for (int z = 0; z < bounds.y; z++)
                {
                    //float distance = Mathf.Sqrt((x - bounds.x / 2f) * (x - bounds.x / 2f) + (z - bounds.y / 2f) * (z - bounds.y / 2f));

                    //// Calculate the Y position based on the distance
                    //float yPos = Mathf.Sin(distance * Mathf.PI / (bounds.x / 2f)) * 10f; // Example formula, adjust as needed

                    Vector3 spawnPosition = new Vector3(prefabSize.x * (x - bounds.x / 2f + 0.5f),
                                                        //yPos,
                                                        0f,
                                                        prefabSize.z * (z - bounds.y / 2f + 0.5f));
                    GameObject go = Instantiate(groundPrefab, spawnPosition, Quaternion.identity, parentRootTransform);
                    go.name += $" {spawnPosition}";

                    // DISABLE BY DEFAULT
                    go.SetActive(false);

                    groundTiles.Add(go);
                    OnItmeSpawned(go);
                }
            }

            var playerScript = FindFirstObjectByType<LevelChunkLoading.PlayerScript>();

            if(playerScript != null)
                playerScript.OnSpawnCompleted_TEMP();
        }

        public void OnItmeSpawned(GameObject itemGO)
        {
            ISpacialData2D spacialData2D = itemGO.GetComponent<ISpacialData2D>();
            quadTree.AddData(spacialData2D);
        }
    }

}
