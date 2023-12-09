using System.Collections;
using UnityEngine;
using Unity.Netcode;


public class Game : NetworkBehaviour
{
    public static Game Instance { get; private set; }
    
    private Deck _deck;

    [SerializeField] private Player _player;

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
        StartCoroutine(DealAfterDelay(10f));
    }

    private IEnumerator DealAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        S_Deal();
        InitHand(_player, 3);
    }

    private void InitDeck(int[] deck)
    {
        _deck = new Deck(deck);
        
        if(IsServer)
            Debug.Log($"Deck created: {string.Join(", ", deck)}.");
    }

    private void InitHand(Player player, int amount)
    {
        if (player == null)
        {
            Debug.LogError("Player is null in InitHand method.");
            return;
        }

        if (player.Cards == null || player.Cards.Length < amount)
        {
            Debug.LogError("Player cards array is not properly initialized.");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            player.Cards[i] = _deck.PullCard();
        }

        if (IsServer)
        {
            //Debug.Log($"Player cards: {player.Cards[0].Value}, {player.Cards[1].Value}, {player.Cards[2].Value}.");
            Debug.Log($"Deck amount: {_deck.Cards.Count}");
        }
    }


    #region Server
    private void S_Deal()
    {
        if (!IsServer)
        {
            Debug.Log("Server: Down");
            return;
        }
        
        _deck = new Deck();
        DealClientRpc(CardConverter.GetCodedCards(_deck.Cards));
        
        Debug.Log($"Server: has made {_deck.Cards.Count} cards");
    }
    #endregion
    
    #region RPC
    [ClientRpc]
    private void DealClientRpc(int[] deck)
    {
        InitDeck(deck);
    }
    #endregion
}
