using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
public class Hand : NetworkBehaviour
{
    [SerializeField] private Image[] cardImages;
    [SerializeField] private Player player;
    [SerializeField] private GameObject cardPrefab;
    
    private static Game Game => Game.Instance;
    
    private void OnEnable()
    {
        // Subscribe to the OnCardsDealt event
        Game.OnCardsDealt += UpdateCardsUIClientRpc;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event to avoid memory leaks
        Game.OnCardsDealt -= UpdateCardsUIClientRpc;
    }
    
    
    [ClientRpc]
    private void SpawnCardSlotsClientRpc(int amount)
    {
        // Clear existing card slots
        ClearCardSlots();

        // Ensure that the cardImages array has enough slots
        if (cardImages.Length < amount)
        {
            Debug.LogError("Not enough card image slots for the given amount.");
            return;
        }

        // Instantiate cardPrefab GameObjects
        for (int i = 0; i < amount; i++)
        {
            GameObject cardInstance = Instantiate(cardPrefab, transform);
            Image cardImage = cardInstance.GetComponent<Image>();

            // Ensure that the cardPrefab has an Image component
            if (cardImage == null)
            {
                Debug.LogError("The cardPrefab is missing the Image component.");
                return;
            }

            // Assign the Image component to the cardImages array
            cardImages[i] = cardImage;
        }
    }
    
    [ClientRpc]
    private void UpdateCardsUIClientRpc(ulong playerId)
    {
        // Update the reference to the local player
        player = Game.LocalPlayer;
        
        if (player == null)
        {
            Debug.LogError($"Player not found with ID: {playerId}");
            return;
        }
        
        Debug.Log($"Player {player.OwnerClientId} was found");

        if (player != null)
        {
            if (player.Cards == null)
            {
                player.InitializeCards(cardImages.Length);
                return;
            }
            
            SpawnCardSlotsClientRpc(player.Cards.Length);

            // Update the card images based on the player's hand
            for (int i = 0; i < player.Cards.Length; i++)
            {
                string path = $"Sprites/Cards/{(int)player.Cards[i].Suit}_{(int)player.Cards[i].Value}";
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite == null)
                {
                    Debug.LogError($"Sprite not found at path: {path}");
                }
                else
                {
                    cardImages[i].sprite = sprite;
                }
            }
        }
    }
    
    // Helper method to clear existing card slots
    private void ClearCardSlots()
    {
        foreach (Image cardImage in cardImages)
        {
            if (cardImage != null)
            {
                Destroy(cardImage.gameObject);
            }
        }
    }
}