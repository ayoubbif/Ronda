using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public string NickName => _nickName.Value.ToString();
    private readonly NetworkVariable<FixedString32Bytes> _nickName = new();
    public Card[] Cards { get; set; }

    public uint Score => _score.Value;
    private readonly NetworkVariable<uint> _score = new();

    private static Game Game => Game.Instance;
    

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            SetSeatServerRpc();
        }
    }
    
    public void InitializeCards(int size)
    {
        Cards = new Card[size];
    }

    public void SetCards(Card[] cards)
    {
        Cards = cards;
    }

    [ClientRpc]
    private void SetSeatClientRpc()
    {
        // Add player on all clients
        Game gameInstance = Game.Instance;
        if (gameInstance != null)
        {
            gameInstance.AddPlayer(this);
            Debug.Log($"Player added to _players list. Player count: {gameInstance.Players.Count}");
        }
        else
        {
            Debug.LogWarning("Game.Instance is null.");
        }
    }

    [ServerRpc]
    private void SetSeatServerRpc()
    {
        // Call the client RPC to add the player on all clients
        SetSeatClientRpc();
    }

}