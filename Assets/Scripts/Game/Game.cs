using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;


public class Game : NetworkBehaviour
{
    public static Game Instance { get; private set; }
    public Action<ulong> OnCardsDealt;
    private Deck _deck;
    
    public List<Player> Players => players.ToList();
    [SerializeField] private List<Player> players = new();
    
    private bool _isDeckInitialized;
    
    public Player LocalPlayer => GetLocalPlayer();

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
        Debug.Log($"Player added to _players list. Player count: {players.Count}");
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
        var numCardsToDeal = _deck.Cards.Count == 4 ? 2 : 3;
        Debug.Log($"Server: numCardsToDeal is {numCardsToDeal}");

        foreach (var player in players)
        {
            Debug.Log($"Server: Dealing {numCardsToDeal} cards to player {player.OwnerClientId}");
            
            // Ensure that the player.Cards array is initialized with the correct size
            player.InitializeCards(numCardsToDeal);

            for (int i = 0; i < numCardsToDeal; i++)
            {
                player.Cards[i] = _deck.PullCard();
                Debug.Log($"Server: Pulling card {i + 1} for player {player.OwnerClientId}");
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