using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardContainer : MonoBehaviour
{
    [SerializeField] [Range(0f, 90f)] private float rotationAngle;
    [SerializeField] private float maxHeightDisplacement;
    [SerializeField] private bool forceContainerFit;
    [SerializeField] private AnimationSpeedConfig animationSpeedConfig;
    [SerializeField] private CardAlignment alignment = CardAlignment.Center;

    private List<CardController> _cards = new();

    private RectTransform _rect;
    
    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        InitCards();
    }

    private void InitCards()
    {
        SetupCards();
        SetCardsAnchor();
    }

    private void UpdateCards()
    {
        if (transform.childCount != _cards.Count)
        {
            InitCards();
        }
        
        if (_cards.Count == 0) {
            return;
        }
        
        SetCardsRotation();
        SetCardsPosition();
    }

    private void Update()
    {
        UpdateCards();
    }

    private void SetupCards()
    {
        _cards.Clear();
        foreach (Transform card in transform)
        {
            var cardController = card.GetComponent<CardController>();
            if (cardController == null)
            {
                cardController = card.gameObject.AddComponent<CardController>();
            }
            
            _cards.Add(cardController);

            cardController.animationSpeedConfig = animationSpeedConfig;
            cardController.container = this;
        }
    }

    private float GetCardVerticalDisplacement(int index)
    {
        if (_cards.Count < 3) return 0;

        return maxHeightDisplacement *
               (1 - Mathf.Pow(index - (_cards.Count - 1) / 2f, 2) / Mathf.Pow((_cards.Count - 1) / 2f, 2));
    }

    private float GetCardRotation(int index)
    {
        if (_cards.Count < 3) return 0;

        return -rotationAngle * (index - (_cards.Count - 1) / 2f) / ((_cards.Count - 1) / 2f);
    }

    private void SetCardsRotation()
    {
        for (var i = 0; i < _cards.Count; i++)
        {
            _cards[i].targetRotation = GetCardRotation(i);
            _cards[i].targetVerticalDisplacement = GetCardVerticalDisplacement(i);
        }
    }

    private void SetCardsPosition()
    {
        // Compute the total width of all the cards in global space
        var cardsTotalWidth = _cards.Sum(card => card.Width * card.transform.lossyScale.x);
        // Compute the width of the container in global space
        var containerWidth = _rect.rect.width * transform.lossyScale.x;
        if (forceContainerFit && cardsTotalWidth > containerWidth)
        {
            DistributeChildrenToFitContainer(cardsTotalWidth);
        }
        else
        {
            DistributeChildrenWithoutOverlap(cardsTotalWidth);
        }
    }

    private void DistributeChildrenToFitContainer(float childrenTotalWidth)
    {
        // Get container width
        var width = _rect.rect.width * transform.lossyScale.x;
        
        // Get the distance between each child 
        var distanceBetweenChildren = (width - childrenTotalWidth) / (_cards.Count - 1);
        
        // Set all children's positions to be evenly spaced out 
        var currentX = transform.position.x - width / 2;
        foreach (CardController card in _cards)
        {
            var adjustedChildWidth = card.Width * card.transform.lossyScale.x;
            card.targetPosition = new Vector2(currentX + adjustedChildWidth / 2, transform.position.y);
            currentX += adjustedChildWidth + distanceBetweenChildren;
        }
    }
    
    private void DistributeChildrenWithoutOverlap(float childrenTotalWidth) {
        var currentPosition = GetAnchorPositionByAlignment(childrenTotalWidth);
        foreach (CardController child in _cards) {
            var adjustedChildWidth = child.Width * child.transform.lossyScale.x;
            child.targetPosition = new Vector2(currentPosition + adjustedChildWidth / 2, transform.position.y);
            currentPosition += adjustedChildWidth;
        }
    }

    private float GetAnchorPositionByAlignment(float childrenWidth) {
        var containerWidthInGlobalSpace = _rect.rect.width * transform.lossyScale.x;
        switch (alignment) {
            case CardAlignment.Left:
                return transform.position.x - containerWidthInGlobalSpace / 2;
            case CardAlignment.Center:
                return transform.position.x - childrenWidth / 2;
            case CardAlignment.Right:
                return transform.position.x + containerWidthInGlobalSpace / 2 - childrenWidth;
            default:
                return 0;
        }
    }

    private void SetCardsAnchor()
    {
        foreach (var card in _cards)
        {
            card.SetAnchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        }
    }
}
