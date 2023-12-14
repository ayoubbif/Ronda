using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Table : MonoBehaviour
{
    public List<Card> Cards => cards.ToList();
    [SerializeField] private List<Card> cards;

    public Table(List<Card> cards)
    {
        this.cards = cards;
    }

    public void AddCardToTable(Card card)
    {
        cards.Add(card);
    }

    public void RemoveCardFromTable(Value value, Suit suit)
    {
        Card cardToRemove = cards.Find(c => c.Value == value && c.Suit == suit);

        if (cardToRemove != null)
        {
            cards.Remove(cardToRemove);
        }
        else
        {
            Debug.Log("Attempted to remove a card not present in the hand.");
        }
    }
}

