using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Table : MonoBehaviour
{
    public List<Card> Cards => _cards.ToList();
    [SerializeField] private List<Card> _cards;

    public Table(List<Card> cards)
    {
        _cards = cards;
    }
}
