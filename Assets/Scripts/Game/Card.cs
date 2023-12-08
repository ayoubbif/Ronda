using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class Card : INetworkSerializable
{
    public Suit Suit => _suit;
    private Suit _suit;

    public Value Value => _value;
    private Value _value;

    public Sprite CardSprite => _cardSprite;
    private Sprite _cardSprite;
    
    public Card() { }

    public Card(Suit suit, Value value)
    {
        _suit = suit;
        _value = value;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _suit);
        serializer.SerializeValue(ref _value);
    }
    
    public void LoadSprite()
    {
        // Load sprite only if it hasn't been loaded before
        if (CardSprite == null)
        {
            string spriteName = $"{(int)Suit}_{(int)Value}";
            _cardSprite = Resources.Load<Sprite>($"CardSprites/{spriteName}");

            if (CardSprite == null)
            {
                Debug.LogError($"Sprite not found for card: {spriteName}");
            }
        }
    }
}
