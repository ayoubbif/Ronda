using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;


public class Game : NetworkBehaviour
{
    public static Game Instance { get; private set; }
    
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform hand;
    
    public List<Player> Players => players.ToList();
    [SerializeField] private List<Player> players = new();
    
    private Image _cardImage;
    private int _numCardsToDeal;
    private Action<ulong> OnCardsDealt;
    
    private Deck _deck;
    private bool _isDeckInitialized;

    [SerializeField] private Table _table;
    
    
    public Player LocalPlayer => GetLocalPlayer();

    private void OnEnable()
    {
        OnCardsDealt += SpawnCardsClientRpc;
    }
    
    private void OnDisable()
    {
        OnCardsDealt -= SpawnCardsClientRpc;
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
    
    private void Start()
    {
        _cardImage = cardPrefab.GetComponent<Image>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartCoroutine(DealAfterDelay(1f));
        }
    }

    public void AddPlayer(Player newPlayer)
    {
        players.Add(newPlayer);
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
    
    private IEnumerator DealAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        S_Deal();
    }
    
    #region Server
    private void S_Deal()
    {
        if (IsServer == false)
        {
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

        // Deal cards on the server
        DealFromDeck();
        
        Debug.Log($"Server: There are {_deck.Cards.Count} cards left in deck");
    }
    
    #endregion
    
    #region RPC
    
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
    }

    [ClientRpc]
    private void SpawnCardsClientRpc(ulong playerId)
    {
        // Check if the current instance is the local player's client
        if (LocalPlayer.OwnerClientId == playerId)
        {
            if (LocalPlayer.Cards == null)
            {
                LocalPlayer.InitializeCards(_numCardsToDeal);
                return;
            }
        
            // Update the card images based on the player's hand
            for (int i = 0; i < LocalPlayer.Cards.Length; i++)
            {
                var cardInstance = Instantiate(cardPrefab, hand);
                var cardImage = cardInstance.GetComponent<Image>();
                
                Suit cardSuit = LocalPlayer.Cards[i].Suit;
                Value cardValue = LocalPlayer.Cards[i].Value;


                string path = $"Sprites/Cards/{(int)cardSuit}_{(int)cardValue}";
                Sprite sprite = Resources.Load<Sprite>(path);

                if (sprite == null)
                {
                    Debug.LogError($"Sprite not found at path: {path}");
                }
                else
                {
                    cardImage.sprite = sprite;
                    cardInstance.name = $"{(int)cardSuit}_{(int)cardValue}";
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyServerOnCardPlayedServerRpc(int codedCard, ulong playerId)
    {
        Debug.Log($"Server received NotifyServerOnCardPlayedServerRpc. Coded Card: {codedCard}");
    
        // Notify the other player about the played card
        NotifyServerOnCardPlayedClientRpc(codedCard, playerId);
    }

    [ClientRpc]
    private void NotifyServerOnCardPlayedClientRpc(int codedCard, ulong playerId)
    {
        // Check if the player invoking the method is the local player
        if (LocalPlayer.OwnerClientId == playerId) return;
        {
            // Spawn the played card on the table for other players
            GameObject playedCardObject = Instantiate(cardPrefab, _table.transform);
            Image playedCardImage = playedCardObject.GetComponent<Image>();

            // Decode the coded card to get the suit and value (assuming you have a CardConverter class)
            Card playedCard = CardConverter.DecodeCodedCard(codedCard);

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

            _table.AddCardToTable(playedCard);
        }
    }


    #endregion
    
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

            for (int i = 0; i < _numCardsToDeal; i++)
            {
                player.Cards[i] = _deck.PullCard();
            }

            // Inform the clients about the dealt cards
            SetPlayersCardsClientRpc(player.OwnerClientId, player.Cards);
        }
    }
    
    private void SetPlayersCards(ulong playerId, Card[] cards)
    {
        Player player = players.FirstOrDefault(x => x != null && x.OwnerClientId == playerId);

        if (player == null)
        {
            return;
        }
        
        player.SetCards(cards);
        OnCardsDealt?.Invoke(player.OwnerClientId);
    }
}