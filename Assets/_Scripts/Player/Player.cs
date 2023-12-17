using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public Card[] Cards { get; set; }
    
    public string NickName => _nickName.Value.ToString();
    private readonly NetworkVariable<FixedString32Bytes> _nickName = new();
    
    public uint Score
    {
        get => _score.Value;
        set => _score.Value = value;
    }

    private readonly NetworkVariable<uint> _score = new();

    private static Game Game => Game.Instance;
    
    public List<Card> CardsInHand
    {
        get => cardsInHand.ToList();
        set => cardsInHand = value.ToList();
    }

    [SerializeField] private List<Card> cardsInHand;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            AddPlayerServerRpc();
        }
    }
    
    private void Start()
    {
        _score.OnValueChanged += ScoreChanged;
    }

    public override void OnDestroy()
    {
        _score.OnValueChanged -= ScoreChanged;
    }

    private static void ScoreChanged(uint oldValue, uint newValue)
    {
        // Notify Game instance to update score UI
        Game.Instance.UpdateScoreUI();
    }
    
    public void InitializeCards(int size)
    {
        Cards = new Card[size];
    }

    public void SetCards(Card[] cards)
    {
        Cards = cards;
    }

    public void AddCardsToHand(List<Card> cards)
    {
        cardsInHand = cards;
    }
    
    public void AddScore(uint score)
    {
        _score.Value += score;
    }
    
    public void RemoveCardFromHand(Value value, Suit suit)
    {
        Card cardToRemove = cardsInHand.Find(c => c.Value == value && c.Suit == suit);

        if (cardToRemove != null)
        {
            cardsInHand.Remove(cardToRemove);
        }
    }
    
    [ClientRpc]
    private void AddPlayerClientRpc()
    {
        Game.AddPlayer(this);
        Debug.Log($"Player count: {Game.Players.Count}");
    }

    [ServerRpc]
    private void AddPlayerServerRpc()
    {
        // Call the client RPC to add the player on all clients
        AddPlayerClientRpc();
    }
}
