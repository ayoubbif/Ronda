using UnityEditor;


[CustomEditor(typeof(Table))]
public class TableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Table table = (Table)target;

        EditorGUILayout.LabelField("Card Count", table.Cards.Count.ToString());

        for (int i = 0; i < table.Cards.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Card {i + 1} Value", table.Cards[i].Value.ToString());
            EditorGUILayout.LabelField($"Card {i + 1} Suit", table.Cards[i].Suit.ToString());
            EditorGUILayout.EndHorizontal();
        }

        base.OnInspectorGUI();
    }
}
