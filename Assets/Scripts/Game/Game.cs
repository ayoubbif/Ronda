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
    public Action<ulong> OnCardsDealt;
    private Deck _deck;
    private bool _isDeckInitialized;
    
    public List<Player> Players => players.ToList();
    [SerializeField] private List<Player> players = new();
    
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform hand;
    
    private Image _cardImage;
    private int _numCardsToDeal;
    
    [SerializeField] private Table table;
    public List<Card> CardsOnTable => table.Cards.ToList();
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
        Debug.Log($"ClientRpc called. PlayerId: {playerId}, Cards: {cards.Length}");
        SetPlayersCards(playerId, cards);
    }

    [ClientRpc]
    private void SpawnCardsClientRpc(ulong playerId)
    {
        // Check if the current instance is the local player's client
        if (IsClient && LocalPlayer.OwnerClientId == playerId)
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

                string path = $"Sprites/Cards/{(int)LocalPlayer.Cards[i].Suit}_{(int)LocalPlayer.Cards[i].Value}";
                Sprite sprite = Resources.Load<Sprite>(path);

                if (sprite == null)
                {
                    Debug.LogError($"Sprite not found at path: {path}");
                }
                else
                {
                    // Assuming each card has its own Image component
                    cardImage.sprite = sprite;
                }
            }
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
        Debug.Log($"Server: _deck.Cards.Count is {_deck.Cards.Count}");
        _numCardsToDeal = _deck.Cards.Count == 4 ? 2 : 3;
        Debug.Log($"Server: numCardsToDeal is {_numCardsToDeal}");

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
            
            Debug.Log($"Server: Player ('{player.OwnerClientId}') received: {player.Cards.Length}.");
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