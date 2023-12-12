using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Image _cardImage;
    private Transform _parent;
    private Table _table;
    private Card _card;

    [SerializeField] private Value _value;
    [SerializeField] private Suit _suit;
    
    private static Game Game => Game.Instance;

    private void Awake()
    {
        _cardImage = GetComponent<Image>();
        _parent = transform.parent;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CardCannotBeDragged())
        {

        }
        else
        {
            SetCardDragProperties();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragCardWithPointer(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (CardCannotBeDragged())
        {

        }
        else
        {
            _cardImage.raycastTarget = true;
            AnalyzePointerUp(eventData);
        }
    }

    private bool CardCannotBeDragged()
    {
        return _parent != null && _parent.name == "Table";
    }

    private void SetCardDragProperties()
    {
        transform.SetParent(transform.root);
        _cardImage.raycastTarget = false;
    }

    private void DragCardWithPointer(PointerEventData eventData)
    {
        if(transform.parent == _parent) return;
        transform.position = eventData.position;
    }

    private void AnalyzePointerUp(PointerEventData eventData)
    {
        if (IsPointerReleasedOnTable(eventData))
        {
            PlayCardOnTable(eventData.pointerEnter.transform);
        }
        else
        {
            ReturnCardToHand();
        }
    }

    private bool IsPointerReleasedOnTable(PointerEventData eventData)
    {
        return eventData.pointerEnter != null && eventData.pointerEnter.name == "Table";
    }

    private void PlayCardOnTable(Transform table)
    {
        SetCardParentAndPosition(table);
        
        _value = GetCardValue().Value;
        _suit = GetCardValue().Suit;

        var localPlayer = Game.LocalPlayer.OwnerClientId;
                
        Game.NotifyServerOnCardPlayedServerRpc(CardConverter.GetCodedCard(_card), localPlayer);
        
        GetComponentInParent<Table>().AddCardToTable(_card);
    }
    
    private Card GetCardValue()
    {
        // Extracting card suit and value from the GameObject's name
        string[] nameParts = gameObject.name.Split('_');
        if (nameParts.Length == 2)
        {
            if (int.TryParse(nameParts[0], out int suitValue) && int.TryParse(nameParts[1], out int valueValue))
            {
                _card = new Card((Suit)suitValue, (Value)valueValue);
            }
        }
        
        return _card;
    }

    private void ReturnCardToHand()
    {
        SetCardParentAndPosition(_parent);
    }

    private void SetCardParentAndPosition(Transform parent)
    {
        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        _parent = parent;
    }
}