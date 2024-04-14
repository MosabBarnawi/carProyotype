using UnityEngine;

namespace BarnoGames.QuadTree.Sample
{
    [CreateAssetMenu(fileName = "QuadTreeDebuggerSettings", menuName = "BarnoGames/QuadTree/DebugSettingsAsset")]
    public class QuadTreeDebuggerSO : ScriptableObject
    {
        public Color InRangeColor = Color.red;
        public Color OutOfRanageColor = Color.black;
    }

}
