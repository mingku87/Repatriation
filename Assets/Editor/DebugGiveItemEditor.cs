#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugGiveItem))]
public class DebugGiveItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // itemId, count, autoInit 등 기본 필드 표시

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Give (Play Mode)", GUILayout.Height(28)))
            {
                ((DebugGiveItem)target).Give();
            }
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("플레이 모드에서만 지급 버튼이 활성화됩니다.", MessageType.Info);
    }
}
#endif
