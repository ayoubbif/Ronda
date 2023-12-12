using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public Card[] Cards { get; set; }
    
    public string NickName => _nickName.Value.ToString();
    private readonly NetworkVariable<FixedString32Bytes> _nickName = new();
    
    public uint Score => _score.Value;
    private readonly NetworkVariable<uint> _score = new();

    private static Game Game => Game.Instance;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsOwner)
        {
            AddPlayerServerRpc();
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
