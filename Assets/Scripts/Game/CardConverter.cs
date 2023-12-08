using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public static class CardConverter
{
    private const char Separator = '_';

    public static IEnumerable<Card> GetCards(IEnumerable<int> codedCards)
    {
        Queue<Card> cards = new();
        
        foreach (int card in codedCards)
        {
            Value value = (Value)(card % 100);
            Suit suit = (Suit)((card - (int)value) / 100);
            cards.Enqueue(new Card(suit, value));
        }

        return cards;
    }

    public static IEnumerable<Card> GetCards(string codedCards)
    {
        string[] stringCards = codedCards.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        return GetCards(stringCards.Select(int.Parse));
    }
    
    // 212: Suit = 2, Value = 12 which means Rey de Espadas
    public static int[] GetCodedCards(IEnumerable<Card> cards)
    {
        List<int> codedCards = new();
        foreach (Card card in cards)
        {
            var suit = (int)card.Suit;
            var value = (int)card.Value;
            codedCards.Add(suit * 100 + value);
        }

        return codedCards.ToArray();
    }

    public static string GetCodedCardsString(IEnumerable<Card> cards)
    {
        var codedCards = string.Empty;
        foreach (Card card in cards)
        {
            var suit = (int)card.Suit;
            var value = (int)card.Value;
            codedCards += (suit * 100 + value) + "_";
        }

        return codedCards;
    }
}
