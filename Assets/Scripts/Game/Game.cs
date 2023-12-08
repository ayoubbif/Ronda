using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;

public class Game : NetworkBehaviour
{
    public static Game Instance { get; private set; }
    
    public string CodedBoardCardsString => _codedBoardCardsString.Value.ToString();
    private readonly NetworkVariable<FixedString64Bytes> _codedBoardCardsString = new();

    [ReadOnly] [SerializeField] private Table _table;
    public List<Card> TableCards => _table.Cards.ToList();

    private Deck _deck;

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
    
}
