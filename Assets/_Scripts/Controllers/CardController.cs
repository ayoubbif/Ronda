using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;

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

        _card = CardConverter.GetCardValueFromGameObject(gameObject);
        _value = _card.Value;
        _suit = _card.Suit;

        var localPlayerId = Game.LocalPlayer.OwnerClientId;

        Game.OnCardPlayedServerRpc(CardConverter.GetCodedCard(_card), localPlayerId);
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