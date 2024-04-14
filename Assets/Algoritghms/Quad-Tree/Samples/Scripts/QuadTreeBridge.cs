using System.Collections.Generic;
using UnityEngine;

namespace BarnoGames.QuadTree.Sample
{
    public class QuadTreeBridge : MonoBehaviour
    {
        [SerializeField] QuadTree LinkedQuadTree;

        public void On2DBoundsCalculated(Rect bounds)
        {
            LinkedQuadTree.PrepareTree(bounds);
        }

        public void OnItmeSpawned(GameObject itemGO)
        {
            ISpacialData2D spacialData2D = itemGO.GetComponent<ISpacialData2D>();
            LinkedQuadTree.AddData(spacialData2D);
        }

        public void OnAllItemsSpwned(List<GameObject> list)
        {
            // CAN ADD COMPLETE LIST TO QUAD TREE TO PROESS ALL AT ONCES

            LinkedQuadTree.ShowStats();
        }
    }
}
