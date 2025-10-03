#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugGiveItem))]
public class DebugGiveItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // itemId, count, autoInit �� �⺻ �ʵ� ǥ��

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Give (Play Mode)", GUILayout.Height(28)))
            {
                ((DebugGiveItem)target).Give();
            }
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("�÷��� ��忡���� ���� ��ư�� Ȱ��ȭ�˴ϴ�.", MessageType.Info);
    }
}
#endif
