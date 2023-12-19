using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CardController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Image _cardImage;
    private Transform _parent;
    private Table _table;
    private Card _card;
    
    private static Game Game => Game.Instance;

    public float targetRotation;
    public Vector2 targetPosition;
    public float targetVerticalDisplacement;

    public AnimationSpeedConfig animationSpeedConfig;

    public CardContainer container;

    private RectTransform _rect;
    private bool _releasedOnTable;
    private bool _isDragged;
    
    public float Width => _rect.rect.width * _rect.localScale.x;

    private void Awake()
    {
        _cardImage = GetComponent<Image>();
        _rect = GetComponent<RectTransform>();
        _parent = transform.parent;
    }

    private void Update()
    {
        if (_releasedOnTable)
        {
            SetCardControllerPropsToZero();
            return;
        }
        UpdatePosition();
        UpdateRotation();
    }

    private void SetCardControllerPropsToZero()
    {
        targetRotation = 0;
        targetPosition = Vector2.zero;
        targetVerticalDisplacement = 0;
        _rect.rotation = Quaternion.identity;
    }

    public void SetAnchor(Vector2 min, Vector2 max)
    {
        _rect.anchorMin = min;
        _rect.anchorMax = max;
    }

    private void UpdatePosition()
    {
        if (_isDragged)
        {
            SetCardControllerPropsToZero();
            return;
        }
        
        var target = new Vector3(targetPosition.x, targetPosition.y + targetVerticalDisplacement, 0);
        var distance = Vector2.Distance(_rect.position, target);
        var repositionSpeed = _rect.position.y > target.y || _rect.position.y < 0
            ? animationSpeedConfig.releasePosition
            : animationSpeedConfig.position;
        
        if (distance.Equals(0))
        {
            return; 
        }
            
        _rect.position = Vector2.Lerp(_rect.position, target,
            repositionSpeed / distance * Time.deltaTime);
    }

    private void UpdateRotation()
    {
        var currentAngle = _rect.rotation.eulerAngles.z;

        currentAngle = currentAngle < 0 ? currentAngle + 360 : currentAngle;

        var tempTargetRotation = targetRotation;

        tempTargetRotation = tempTargetRotation < 0 ? tempTargetRotation + 360 : tempTargetRotation;
        var deltaAngle = Mathf.Abs(currentAngle - tempTargetRotation);
        
        if(!(deltaAngle > 0.01)) return;

        var adjustedCurrent = deltaAngle > 180 && currentAngle < tempTargetRotation
            ? currentAngle + 360
            : currentAngle;
        var adjustedTarget = deltaAngle > 180 && currentAngle > tempTargetRotation
            ? tempTargetRotation + 360
            : tempTargetRotation;
        
        var newDelta = Mathf.Abs(adjustedCurrent - adjustedTarget);

        var nextRotation = Mathf.Lerp(adjustedCurrent, adjustedTarget,
            animationSpeedConfig.rotation / newDelta * Time.deltaTime);

        _rect.rotation = Quaternion.Euler(0, 0, nextRotation);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragged = true;
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
        _isDragged = false;
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
            _releasedOnTable = true;
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