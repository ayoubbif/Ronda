using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class Game : NetworkBehaviour
{
    // Singleton Instance
    public static Game Instance { get; private set; }

    // SerializeField variables
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform playerHand;
    [SerializeField] private Transform enemyHand;
    [SerializeField] private List<Card> straightCards = new();
    [SerializeField] private Table table;
    [SerializeField] private TMP_Text scoreText;

    // Player related variables
    private const int MaxNumPlayers = 2;
    public List<Player> Players => players.ToList();
    [SerializeField] private List<Player> players = new();
    
    // Deck related variables
    private Deck _deck;
    private bool _isDeckInitialized;
    private Card _scoredCard;
    
    // Game logic variables
    private int _numCardsToDeal;
    private Action<ulong> _onCardsDealt;
    private bool _isEmptyHanded;
    private bool _areMatchingCards;
     
    
    public Player LocalPlayer => GetLocalPlayer();

    private void OnEnable()
    {
        _onCardsDealt += SpawnCardsClientRpc;
        _onCardsDealt += SpawnEnemyClientRpc;
    }
    
    private void OnDisable()
    {
        _onCardsDealt -= SpawnCardsClientRpc;
        _onCardsDealt -= SpawnEnemyClientRpc;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    #region Server
    
    private void S_Deal()
    {
        if (IsServer == false && Players.Count < MaxNumPlayers)
        {
            Debug.Log("Not enough players");
            return;
        }

        if (!_isDeckInitialized)
        {
            _deck = new Deck();
            _isDeckInitialized = true;
        }

        // Get the coded cards from the deck
        int[] codedCards = CardConverter.GetCodedCards(_deck.Cards);

        // Inform the clients to initialize the deck
        InitDeckClientRpc(codedCards);
        
        // Deal new cards
        DealFromDeck();
        
        Debug.Log($"Server: There are {_deck.Cards.Count} cards left in deck");
    }
    
    #endregion
    
    #region ClientRPC
    [ClientRpc]
    private void InitDeckClientRpc(int[] deck)
    {
        // Initialize the deck on the client
        InitDeck(deck);
    }

    [ClientRpc]
    private void SetPlayersCardsClientRpc(ulong playerId, Card[] cards)
    {
        SetPlayersCards(playerId, cards);
        
        if (LocalPlayer.OwnerClientId == playerId)
        {
            LocalPlayer.AddCardsToHand(cards.ToList());

            _isEmptyHanded = false;
        }
    }

    [ClientRpc]
    private void SpawnCardsClientRpc(ulong playerId)
    {
        // Check if the current instance is the local player's client
        if (IsClient && LocalPlayer.OwnerClientId == playerId)
        {
            InitLocalPlayerCards();
        
        }
    }
        
    [ClientRpc]
    private void SpawnEnemyClientRpc(ulong playerId)
    {
        // Check if the current instance is the local player's client
        if (IsClient && LocalPlayer.OwnerClientId == playerId)
        {
            InitEnemyPlayerCards();
        }
    }
    [ClientRpc]
    private void NotifyServerOnCardPlayedClientRpc(int codedCard, ulong playerId)
    {
        // Decode the coded card to get the suit and value
        Card playedCard = CardConverter.DecodeCodedCard(codedCard);
        
        // Remove the played card from the player's hand
        LocalPlayer.RemoveCardFromHand(playedCard.Value, playedCard.Suit);
        
        CheckMatchingCardsOnTableClientRpc(playedCard);

        // Check if the player invoking the method is the local player
        if (LocalPlayer.OwnerClientId == playerId)
        {
            return;
        }
        
        // Spawn the played card on the table for other players
        GameObject playedCardObject = Instantiate(cardPrefab, table.transform);
        Image playedCardImage = playedCardObject.GetComponent<Image>();

        // Load the sprite for the played card
        string path = $"Sprites/Cards/{(int)playedCard.Suit}_{(int)playedCard.Value}";
        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogError($"Sprite not found at path: {path}");
        }
        else
        {
            playedCardImage.sprite = sprite;
            playedCardObject.name = $"{(int)playedCard.Suit}_{(int)playedCard.Value}";
        }
        
        _isEmptyHanded = players.All(player => player.CardsInHand.Count == 0);
    }

    [ClientRpc]
    private void CheckMatchingCardsOnTableClientRpc(Card card)
    {
        _areMatchingCards = IsMatchingCardOnTable(card.Value);
    }
    [ClientRpc]
    private void NotifyServerToRemoveMatchingCardsClientRpc(Card card, Card[] sequence)
    {
        // Decode the coded card to get the suit and value
        _scoredCard = card;
        
        // Create a list to store cards that need to be removed from the table
        var cardsToRemove = new HashSet<Card>();
        var cardsOnTable = table.Cards.ToHashSet();

        // Iterate through cards on the table to find matching cards
        foreach (var cardOnTable in cardsOnTable.Where(cardOnTable => cardOnTable.Value == _scoredCard.Value))
        {
            // Add the scored card to the list for removal
            cardsToRemove.Add(_scoredCard);
            // Add matching cards to the list for removal
            cardsToRemove.Add(cardOnTable);

            foreach (var c in sequence)
            {
                cardsToRemove.Add(c);
            }
        }
        
        foreach (var cardToRemove in cardsToRemove)
        {
            table.RemoveCardFromTable(cardToRemove.Suit, cardToRemove.Value);
        }
        
        // Invoke the RPC to set corresponding GameObjects inactive on clients
        SetCardObjectsInactiveClientRpc(cardsToRemove.ToArray());
    }
    [ClientRpc]
    private void SetCardObjectsInactiveClientRpc(Card[] cardsToRemove)
    {
        foreach (var cardToRemove in cardsToRemove)
        {
            // Find the corresponding GameObject in the table and set it inactive
            GameObject cardObject = FindCardObjectOnTable(cardToRemove);
            if (cardObject != null)
            {
                cardObject.SetActive(false);
            }
        }

    }
    [ClientRpc]
    private void AddCardToTableClientRpc(Card card)
    {
        table.AddCardToTable(card);
    }
    
    [ClientRpc]
    public void RemoveCardFromEnemyHandClientRpc(ulong playerId)
    {
        if(NetworkManager.Singleton.LocalClientId != playerId) 
        {
            if(enemyHand.childCount <= 0) return;
            GameObject lastCard = enemyHand.GetChild(enemyHand.childCount - 1).gameObject;
            Destroy(lastCard);
        }
    }
    
    #endregion

    #region ServerRPC
    
    [ServerRpc(RequireOwnership = false)]
    public void OnCardPlayedServerRpc(int codedCard, ulong playerId)
    {
        // Find the player who played the card
        var player = Players.FirstOrDefault(p => p.OwnerClientId == playerId);
        
        // Decode the coded card to get the suit and value
        Card playedCard = CardConverter.DecodeCodedCard(codedCard);
     
        // Check if the player is found
        if (player != null)
        {
            NotifyServerOnCardPlayedClientRpc(codedCard, playerId);
    
            RemoveCardFromEnemyHandClientRpc(playerId);
            
            AddCardToTableClientRpc(playedCard);
            CheckForEmptyHandAndDeal();
            
            var sequenceScore = CheckSequenceScore(playedCard.Value);

            if (_areMatchingCards)
            {
                if(sequenceScore > 0)
                {
                    // Add score if sequence is greater than 0
                    player.AddScore((uint)sequenceScore);
                    Debug.Log($"Player {playerId} scored {sequenceScore} points by sequence. Total score: {player.Score}");
                }
                
                const uint matchedScoreToAdd = 2;
                player.AddScore(matchedScoreToAdd);
                
                Debug.Log($"Player {playerId} scored {matchedScoreToAdd} points. Total score: {player.Score}");
                NotifyServerToRemoveMatchingCardsClientRpc(playedCard, straightCards.ToArray());
            }
        }
        else
        {
            Debug.LogError($"Player not found with ID: {playerId}");
        }
    }
    
    #endregion
    
    public void AddPlayer(Player newPlayer)
    {
        players.Add(newPlayer);
        
        if(Players.Count == 2)
            StartCoroutine(DealAfterDelay(1f));
    }
    private Player GetLocalPlayer()
    {
        Player localPlayer = players.FirstOrDefault(x => x != null && x.IsLocalPlayer);

        if (localPlayer == null)
        {
            Debug.LogError("Local player not found.");
        }

        return localPlayer;
    }
    private void InitDeck(int[] deck)
    {
        _deck = new Deck(deck);
        
        if(IsServer)
            Debug.Log($"Deck created: {string.Join(", ", deck)}.");
    }
    private void DealFromDeck()
    {
        _numCardsToDeal = _deck.Cards.Count == 4 ? 2 : 3;

        foreach (var player in players)
        {
            // Ensure that the player.Cards array is initialized with the correct size
            player.InitializeCards(_numCardsToDeal);

            if(_deck.Cards.Count == 0) return;
            for (int i = 0; i < _numCardsToDeal; i++)
            {
                var card = _deck.PullCard();
                player.Cards[i] = card;
            }

            // Inform the clients about the dealt cards
            SetPlayersCardsClientRpc(player.OwnerClientId, player.Cards);
        }
    }
    private IEnumerator DealAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        S_Deal();
    }
    private void CheckForEmptyHandAndDeal()
    {
        // Check if there are exactly 2 players and both players have empty hands
        if (Players.Count != 2 || !_isEmptyHanded) return;
        
        StartCoroutine(DealAfterDelay(2f));
    }
    private void InitLocalPlayerCards()
    {
        if (LocalPlayer.Cards == null)
        {
            LocalPlayer.InitializeCards(_numCardsToDeal);
            return;
        }

        // Update the card images based on the player's hand
        SpriteConverter.UpdatePlayerCardImages(LocalPlayer.Cards, cardPrefab, playerHand);
    }
    private void InitEnemyPlayerCards()
    {
        if (LocalPlayer.Cards == null)
        {
            LocalPlayer.InitializeCards(_numCardsToDeal);
            return;
        }
        // Update the card images based on the player's hand
        SpriteConverter.UpdateEnemyCardImages(LocalPlayer.Cards, cardPrefab, enemyHand);
    }
    private void SetPlayersCards(ulong playerId, Card[] cards)
    {
        Player player = players.FirstOrDefault(x => x != null && x.OwnerClientId == playerId);

        if (player == null)
        {
            return;
        }
        
        player.SetCards(cards);
        _onCardsDealt?.Invoke(player.OwnerClientId);
    }
    private bool IsMatchingCardOnTable(Value playedCardValue)
    {
        // Iterate through the cards on the table and check if any of them have the same value
        return table.Cards.Any(cardOnTable => cardOnTable.Value == playedCardValue);
    }
    private int CheckSequenceScore(Value playedCardValue)
    {
        straightCards = new List<Card>();

        var sortedCards = table.Cards.OrderBy(x => x.Value).ToList();

        for (int i = 0; i < sortedCards.Count; i++)
        {
            // find the played card in the sorted list
            if (sortedCards[i].Value != playedCardValue) continue;

            var j = i;              
            // look for cards to the right of the played card that continue the sequence
            while (j < sortedCards.Count - 1 && sortedCards[j + 1].Value == sortedCards[j].Value + 1)
            {
                j++;
                straightCards.Add(sortedCards[j]);
            }
        }
        
        return straightCards.Count; 
    }
    private GameObject FindCardObjectOnTable(Card card)
    {
        // Iterate through the children of the table to find the card GameObject
        return (from Transform child in table.transform
            where child.name == $"{(int)card.Suit}_{(int)card.Value}"
            select child.gameObject).FirstOrDefault();
    }
    public void UpdateScoreUI()
    {
        scoreText.text = LocalPlayer.Score.ToString();
    }
}