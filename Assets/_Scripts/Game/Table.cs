using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Table : MonoBehaviour
{
    public List<Card> Cards
    {
        get => cards.ToList();
        set => cards = value.ToList();
    }
    [SerializeField] private List<Card> cards;

    public Table(List<Card> cards)
    {
        this.cards = cards;
    }

    public void AddCardToTable(Card card)
    {
        cards.Add(card);
    }

    public void RemoveCardFromTable(Suit suit, Value value)
    {
        Card cardToRemove = cards.Find(c => c.Value == value && c.Suit == suit);

        if (cardToRemove != null)
        {
            cards.Remove(cardToRemove);
        }
    }
    
#if UNITY_EDITOR
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
#endif
}


