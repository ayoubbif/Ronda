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
    
    public bool HasCardBeenPlayed(Card card)
    {
        return cards.Contains(card);
    }
}
