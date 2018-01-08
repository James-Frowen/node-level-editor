using UnityEngine;
using UnityEditor;

namespace NodeLevelEditor
{
    public static class GUIHelper
    {
        public static void HorizontalLine()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
    }
}