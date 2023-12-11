using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public static class CardConverter
{
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
}