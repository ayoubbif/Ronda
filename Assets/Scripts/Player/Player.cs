using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public string NickName => _nickName.Value.ToString();
    private readonly NetworkVariable<FixedString32Bytes> _nickName = new();
    
    public Card[] Cards { get; private set; }

    private static Game Game => Game.Instance;
    
    private void Awake()
    {
        // Initialize the Cards array with the desired size
        Cards = new Card[3]; // Assuming you want an array of size 3
    }
}
