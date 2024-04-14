using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

using Random = UnityEngine.Random;

namespace BarnoGames.QuadTree.Sample
{
    public class RandomItemSpawner : MonoBehaviour
    {
        public enum EMode
        {
            MODE_2D
        }

        [SerializeField] EMode Mode = EMode.MODE_2D;

        [Header("Spawn Settings"), Space(10)]
        [SerializeField] GameObject PrefabToSpawn;
        [SerializeField] int NumToSpawn = 1000;
        [SerializeField] GameObject SpawnZone;
        [SerializeField] float MinRadius = 0.5f;
        [SerializeField] float MaxRadius = 2f;


        [SerializeField] UnityEvent<Rect> On2DBoundsCalculated = new();
        [SerializeField] UnityEvent<GameObject> OnItemSpawned = new();
        [SerializeField] UnityEvent<List<GameObject>> OnAllItemsSpwned = new();


        private void Start()
        {
            PerformSpawning();
        }

        private void PerformSpawning()
        {
            var spawnBounds = SpawnZone.GetComponent<MeshRenderer>().bounds;

            // clearn up already spawned items
            int childCount = SpawnZone.transform.childCount;
            for (int childIndex = 0; childIndex < childCount; childIndex++)
            {
                Destroy(SpawnZone.transform.GetChild(childIndex));
            }

            List<GameObject> itemsSpawnedList = new(NumToSpawn);

            if (Mode == EMode.MODE_2D)
            {
                // CALCULATE SPAWN BOUNDS
                var spawnRect = new Rect(spawnBounds.min.x, spawnBounds.min.z, spawnBounds.size.x, spawnBounds.size.z);

                On2DBoundsCalculated?.Invoke(spawnRect);

                // SPAWN NEW ITEMS
                for (int index = 0; index < NumToSpawn; ++index)
                {
                    Vector3 spawnPos = new Vector3(Random.Range(spawnRect.xMin, spawnRect.xMax),
                                                   0f,
                                                   Random.Range(spawnRect.yMin, spawnRect.yMax));

                    // SPAWN ON EDGES ONLY
                    //int edgeIndex = Random.Range(0, 4); // 0: top, 1: right, 2: bottom, 3: left

                    //Vector3 spawnPos = Vector3.zero;

                    //switch (edgeIndex)
                    //{
                    //    case 0: // Top edge
                    //        spawnPos = new Vector3(Random.Range(spawnRect.xMin, spawnRect.xMax), 0f, spawnRect.yMax);
                    //        break;
                    //    case 1: // Right edge
                    //        spawnPos = new Vector3(spawnRect.xMax, 0f, Random.Range(spawnRect.yMin, spawnRect.yMax));
                    //        break;
                    //    case 2: // Bottom edge
                    //        spawnPos = new Vector3(Random.Range(spawnRect.xMin, spawnRect.xMax), 0f, spawnRect.yMin);
                    //        break;
                    //    case 3: // Left edge
                    //        spawnPos = new Vector3(spawnRect.xMin, 0f, Random.Range(spawnRect.yMin, spawnRect.yMax));
                    //        break;
                    //}

                    float radius = Random.Range(MinRadius, MaxRadius);

                    GameObject newGO = Instantiate(PrefabToSpawn, spawnPos, Quaternion.identity);
                    newGO.transform.localScale = new Vector3(radius, 1f, radius);
                    newGO.transform.SetParent(SpawnZone.transform);

                    itemsSpawnedList.Add(newGO);
                    OnItemSpawned?.Invoke(newGO);
                }

            }

            OnAllItemsSpwned?.Invoke(itemsSpawnedList);
        }
    }
}
