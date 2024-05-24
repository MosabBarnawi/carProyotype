using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BarnoGames.QuadTree
{
    public interface ISpacialData2D
    {
        Vector2 GetLocation();
        Rect GetBounds();
        float GetRadius();
        void OnVisable();
        void OnInvisable();
    }
}
