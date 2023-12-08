using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class Deck : MonoBehaviour
{
    public List<Card> Cards => _cards.ToList();
    private Queue<Card> _cards;

    public Deck()
    {
        CreateDeck();
        ShuffleDeck();
    }

    private void Start()
    {
        CreateDeck();
        ShuffleDeck();
        LoadCardSprites();
        
        Debug.Log(_cards.Count);
        Debug.Log(_cards.First().Suit);
        Debug.Log(_cards.First().Value);
        Debug.Log($"{(int)_cards.First().Suit}_{(int)_cards.First().Value}_{_cards.First().CardSprite.name}");
    }

    public Deck(IEnumerable<int> cards)
    {
        _cards = new Queue<Card>();
        foreach (int card in cards)
        {
            Value value = (Value)(card % 100);
            Suit suit = (Suit)((card - (int)value) / 100);
            _cards.Enqueue(new Card(suit, value));
        }
    }

    public Card PullCard()
    {
        return _cards.Dequeue();
    }

    public Card[] PullCards(int amount)
    {
        List<Card> cards = new List<Card>();
        for (var i = 0; i < amount && _cards.Count > 0; i++)
        {
            cards.Add(_cards.Dequeue());
        }

        return cards.ToArray();
    }

    private void CreateDeck()
    {
        _cards = new Queue<Card>();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Value value in Enum.GetValues(typeof(Value)))
            {
                Card cardObject = new Card(suit, value);
                _cards.Enqueue(cardObject);
            }
        }
    }

    private void ShuffleDeck()
    {
        // Use a single Random instance to avoid potential issues with seeding.
        Random random = new Random();

        // Fisher-Yates shuffle algorithm.
        Card[] shuffledArray = _cards.ToArray();
        int n = shuffledArray.Length;
        while (n > 1)
        {
            int k = random.Next(n--);
            (shuffledArray[n], shuffledArray[k]) = (shuffledArray[k], shuffledArray[n]);
        }

        _cards = new Queue<Card>(shuffledArray);
    }

    private void LoadCardSprites()
    {
        foreach (Card card in _cards)
        {
            card.LoadSprite();
        }
    }
}
