using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // Network Variables
    private readonly NetworkVariable<FixedString32Bytes> _nickName = new();
    private readonly NetworkVariable<uint> _score = new();
    
    // Game instance
    private static Game Game => Game.Instance; 

    // SerializeField variable
    [SerializeField] private List<Card> cardsInHand;

    // Public Properties
    public Card[] Cards { get; set; }
    public uint Score => _score.Value;
    public List<Card> CardsInHand => cardsInHand.ToList();
    
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
        AddPlayerClientRpc();
    }
}
