#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace BarnoGames.Tools
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public sealed class ReadOnlyDrawer : PropertyDrawer
    {
        // FOR CHILDREN READJUST HEIGHT
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            //EditorGUI.PropertyField(position, property, label);
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

        // FOR ELEMENT UI
        //public override VisualElement CreatePropertyGUI(SerializedProperty property)
        //{
        //    //return base.CreatePropertyGUI(property);

        //    FloatField floatField = new FloatField(property.displayName)
        //    {
        //        value = property.floatValue,
        //    };

        //    floatField.SetEnabled(false);
        //    return floatField;
        //}

        //public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
        //{
        //    GUI.enabled = false;
        //    //EditorGUI.PropertyField(position, property, label, true);
        //    EditorGUI.LabelField(r, label);
        //    EditorGUI.PropertyField(new Rect(r.width - 50 + 14, r.yMin, 50, r.height), property, GUIContent.none, true);
        //    GUI.enabled = true;
        //}
    }
}

#endif