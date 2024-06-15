using System.Collections.Generic;
using UnityEngine;
using BarnoGames.QuadTree;
using BarnoGames.Tools;
using UnityEngine.UIElements;

namespace BarnoGames.LevelChunkLoading
{
    public class SimpleGroundSpawner : MonoBehaviour
    {
        [SerializeField] private KeyCode runtimeGenerateKey = KeyCode.G;
        [SerializeField] private KeyCode runtimeClearKey = KeyCode.C;
        [SerializeField] private GameObject groundPrefab;

        [Header("Prefab Settings"), Space(10)]
        [SerializeField] private Transform Root;
        [SerializeField] private bool isActiveOnSpawn = true;
        [SerializeField, Min(1)] private Vector2Int bounds = new Vector2Int(5, 5);
        List<GameObject> groundTiles = new();

        [ReadOnly, SerializeField] Rect BoundsSizeRect;
        Transform groundPrefabTransform;
        Vector3 BoundsSize;

        [ReadOnly, SerializeField, Space(10)] int numberOfSpawnedObjects = 0;

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
            if (Input.GetKeyDown(runtimeGenerateKey))
                GenerateGround();

            if (Input.GetKeyDown(runtimeClearKey))
                ClearnGroundObjects();
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

        [ContextMenu("Clear Gorund")]
        private void ClearnGroundObjects()
        {
            for (int i = 0; i < groundTiles.Count; i++)
            {
                GameObject item = groundTiles[i];

                if (Application.isPlaying)
                    Destroy(item);
                else
                    DestroyImmediate(item);
            }

            groundTiles = new();
            numberOfSpawnedObjects = groundTiles.Count;
        }

        [ContextMenu("Spawn Gorund")]
        private void GenerateGround()
        {
            for (int i = 0; i < groundTiles.Count; i++)
            {
                GameObject item = groundTiles[i];

                if (Application.isPlaying)
                    Destroy(item);
                else
                    DestroyImmediate(item);
            }

            groundTiles = new();

            Vector3 prefabSize = groundPrefabTransform.localScale;
            Transform parentRootTransform = Root != null ? Root : transform;

            for (int x = 0; x < bounds.x; x++)
            {
                for (int z = 0; z < bounds.y; z++)
                {
                    Vector3 spawnPosition = new Vector3(prefabSize.x * (x - bounds.x / 2f + 0.5f),
                                                        //yPos,
                                                        0f,
                                                        prefabSize.z * (z - bounds.y / 2f + 0.5f));
                    GameObject go = Instantiate(groundPrefab, spawnPosition, Quaternion.identity, parentRootTransform);
                    int number_Debug = x * bounds.y + z + 1;

                    string oldName = go.name;
                    go.name = $"#{number_Debug} {oldName} {spawnPosition}";
                    //go.name += $" {spawnPosition}, #{number_Debug}";

                    // DISABLE BY DEFAULT
                    go.SetActive(isActiveOnSpawn);

                    groundTiles.Add(go);
                    OnItmeSpawned(go);
                }
            }

            numberOfSpawnedObjects = groundTiles.Count;

            //var playerScript = FindFirstObjectByType<LevelChunkLoading.PlayerScript>();

            //if (playerScript != null)
            //    playerScript.OnSpawnCompleted_TEMP();
        }

        public void OnItmeSpawned(GameObject itemGO)
        {
            ISpacialData2D spacialData2D = itemGO.GetComponent<ISpacialData2D>();
            //quadTree.AddData(spacialData2D);
        }
    }

}
