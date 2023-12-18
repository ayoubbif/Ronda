using UnityEngine;
using UnityEngine.UI;

public class SpriteConverter : MonoBehaviour
{
    private static string GetSpriteFilePath(Suit cardSuit, Value cardValue)
    {
        return $"Sprites/Cards/{(int)cardSuit}_{(int)cardValue}";
    }

    private static Sprite LoadSprite(string spritePath)
    {
        var sprite = Resources.Load<Sprite>(spritePath);

        if (sprite == null)
        {
            Debug.LogError($"Sprite not found at path: {spritePath}");
        }

        return sprite;
    }

    private static void UpdatePlayerCardImage(Card card, GameObject cardInstance)
    {
        var cardImage = cardInstance.GetComponent<Image>();
                
        var cardSuit = card.Suit;
        var cardValue = card.Value;

        var spritePath = GetSpriteFilePath(cardSuit, cardValue);
        var sprite = LoadSprite(spritePath);

        cardImage.sprite = sprite;
        cardInstance.name = $"{(int)cardSuit}_{(int)cardValue}";
    }
    
    private static void UpdateEnemyCardImage(GameObject cardInstance)
    {
        var cardImage = cardInstance.GetComponent<Image>();
        const string spritePath = "Sprites/Cards/Back";
        var sprite = LoadSprite(spritePath);
        cardImage.sprite = sprite;
        cardInstance.name = $"Enemy_Card_{cardInstance.GetInstanceID()}";
    }

    public static void UpdatePlayerCardImages(Card[] cards, GameObject cardPrefab, Transform playerHand)
    {
        foreach (var t in cards)
        {
            var cardInstance = Instantiate(cardPrefab, playerHand);
            UpdatePlayerCardImage(t, cardInstance);
        }
    }
    
    public static void UpdateEnemyCardImages(Card[] cards, GameObject cardPrefab, Transform enemyHand)
    {
        for (var i = 0; i < cards.Length; i++)
        {
            var cardInstance = Instantiate(cardPrefab, enemyHand);
            UpdateEnemyCardImage(cardInstance);
        }
    }
}
