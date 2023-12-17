using System;
using System.Collections.Generic;
using UnityEngine;

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
    
    public static int GetCodedCard(Card card)
    {
        var suit = (int)card.Suit;
        var value = (int)card.Value;
        return suit * 100 + value;
    }

    public static Card DecodeCodedCard(int codedCard)
    {
        Suit suit = (Suit)(codedCard / 100);
        Value value = (Value)(codedCard % 100);
        return new Card(suit, value);
    }
    public static Card GetCardValueFromGameObject(GameObject gameObject)
    {
        Card card = null;

        // Extracting card suit and value from the GameObject's name
        string[] nameParts = gameObject.name.Split('_');
        if (nameParts.Length == 2)
        {
            if (int.TryParse(nameParts[0], out int suitValue) && int.TryParse(nameParts[1], out int valueValue))
            {
                card = new Card((Suit)suitValue, (Value)valueValue);
            }
        }

        return card;
    }
}