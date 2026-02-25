using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CardData
{
    public Sprite cardSprite;
    public int cardValue;
    public bool isAce;
}

public class InitialCardDealer : MonoBehaviour
{
    public List<CardData> deckData;
    public Sprite cardBackSprite;
    public GameObject cardPrefab;
    
    public RectTransform playerHandContainer;
    public RectTransform dealerHandContainer;
    public RectTransform deckTransform;

    public Button hitButton;
    public Button standButton;
    public int dealerThreshold = 14;

    public float cardSpacing = 110f;
    public float cardMoveDuration = 0.4f;
    public float delayBetweenCards = 0.2f;
    public float flipDuration = 0.3f;

    private List<int> deck = new List<int>();
    private List<GameObject> playerCards = new List<GameObject>();
    private List<GameObject> dealerCards = new List<GameObject>();

    private List<int> playerHandIndices = new List<int>();
    private List<int> dealerHandIndices = new List<int>();

    private bool isPlayerTurn = false;
    private Image dealerHiddenCardImage;
    private int dealerHiddenCardIndex;
    
    private float resetTimer = 0f;

    void Start()
    {
        hitButton.interactable = false;
        standButton.interactable = false;

        hitButton.onClick.AddListener(OnHitClicked);
        standButton.onClick.AddListener(OnStandClicked);

        InitializeDeck();
        StartCoroutine(DealInitialCardsRoutine());
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            resetTimer += Time.deltaTime;
            if (resetTimer >= 3f)
            {
                resetTimer = 0f;
                ResetSequence();
            }
        }
        else
        {
            resetTimer = 0f;
        }
    }

    public void ResetSequence()
    {
        StopAllCoroutines();

        foreach (GameObject card in playerCards)
        {
            Destroy(card);
        }
        foreach (GameObject card in dealerCards)
        {
            Destroy(card);
        }

        playerCards.Clear();
        dealerCards.Clear();
        playerHandIndices.Clear();
        dealerHandIndices.Clear();

        isPlayerTurn = false;
        dealerHiddenCardImage = null;
        hitButton.interactable = false;
        standButton.interactable = false;

        InitializeDeck();
        StartCoroutine(DealInitialCardsRoutine());
    }

    public void InitializeDeck()
    {
        deck.Clear();
        for (int i = 0; i < deckData.Count; i++)
        {
            deck.Add(i);
        }
        ShuffleDeck();
    }

    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public IEnumerator DealInitialCardsRoutine()
    {
        yield return StartCoroutine(DrawCardRoutine(playerHandContainer, playerCards, playerHandIndices, true, false));
        yield return new WaitForSeconds(delayBetweenCards);
        
        yield return StartCoroutine(DrawCardRoutine(dealerHandContainer, dealerCards, dealerHandIndices, true, false));
        yield return new WaitForSeconds(delayBetweenCards);

        yield return StartCoroutine(DrawCardRoutine(playerHandContainer, playerCards, playerHandIndices, true, false));
        yield return new WaitForSeconds(delayBetweenCards);

        yield return StartCoroutine(DrawCardRoutine(dealerHandContainer, dealerCards, dealerHandIndices, false, true));

        isPlayerTurn = true;
        hitButton.interactable = true;
        standButton.interactable = true;
    }

    public IEnumerator DrawCardRoutine(RectTransform container, List<GameObject> handList, List<int> indexList, bool isVisible, bool isHiddenDealerCard)
    {
        if (deck.Count == 0) yield break;

        int cardIndex = deck[0];
        deck.RemoveAt(0);
        indexList.Add(cardIndex);

        GameObject newCard = Instantiate(cardPrefab, container);
        
        RectTransform cardRect = newCard.GetComponent<RectTransform>();
        
        if (cardRect != null)
        {
            cardRect.localScale = Vector3.one;
        }

        Vector2 targetAnchoredPosition = new Vector2(handList.Count * cardSpacing, 0);
        
        cardRect.anchoredPosition = targetAnchoredPosition;
        Vector3 finalWorldPosition = cardRect.position;
        Vector3 startWorldPosition = deckTransform.position;
        cardRect.position = startWorldPosition;

        handList.Add(newCard);

        Image cardImage = newCard.GetComponent<Image>();
        
        if (cardImage != null)
        {
            cardImage.sprite = cardBackSprite;
            if (isHiddenDealerCard)
            {
                dealerHiddenCardImage = cardImage;
                dealerHiddenCardIndex = cardIndex;
            }
        }

        float elapsedTime = 0f;
        
        while (elapsedTime < cardMoveDuration)
        {
            float t = elapsedTime / cardMoveDuration;
            float easeOutCubic = 1f - Mathf.Pow(1f - t, 3f);
            
            cardRect.position = Vector3.Lerp(startWorldPosition, finalWorldPosition, easeOutCubic);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cardRect.position = finalWorldPosition;
        cardRect.anchoredPosition = targetAnchoredPosition;

        if (isVisible && cardImage != null)
        {
            yield return StartCoroutine(FlipCardRoutine(cardRect, cardImage, deckData[cardIndex].cardSprite));
        }
    }

    public IEnumerator FlipCardRoutine(RectTransform cardRect, Image cardImage, Sprite targetSprite)
    {
        float halfDuration = flipDuration / 2f;
        float elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            float smoothStep = t * t * (3f - 2f * t);
            
            float scaleX = Mathf.Lerp(1f, 0f, smoothStep);
            cardRect.localScale = new Vector3(scaleX, 1f, 1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cardRect.localScale = new Vector3(0f, 1f, 1f);
        cardImage.sprite = targetSprite;

        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            float smoothStep = t * t * (3f - 2f * t);
            
            float scaleX = Mathf.Lerp(0f, 1f, smoothStep);
            cardRect.localScale = new Vector3(scaleX, 1f, 1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cardRect.localScale = Vector3.one;
    }

    public void OnHitClicked()
    {
        if (!isPlayerTurn) return;
        hitButton.interactable = false;
        standButton.interactable = false;
        StartCoroutine(HitRoutine());
    }

    public IEnumerator HitRoutine()
    {
        yield return StartCoroutine(DrawCardRoutine(playerHandContainer, playerCards, playerHandIndices, true, false));

        int playerScore = CalculateHandValue(playerHandIndices);
        
        if (playerScore > 21)
        {
            isPlayerTurn = false;
            Debug.Log("Derrota automatica: El jugador supero los 21 puntos.");
        }
        else
        {
            hitButton.interactable = true;
            standButton.interactable = true;
        }
    }

    public void OnStandClicked()
    {
        if (!isPlayerTurn) return;
        isPlayerTurn = false;
        hitButton.interactable = false;
        standButton.interactable = false;
        StartCoroutine(DealerRoutine());
    }

    public IEnumerator DealerRoutine()
    {
        if (dealerHiddenCardImage != null)
        {
            RectTransform hiddenRect = dealerHiddenCardImage.GetComponent<RectTransform>();
            yield return StartCoroutine(FlipCardRoutine(hiddenRect, dealerHiddenCardImage, deckData[dealerHiddenCardIndex].cardSprite));
        }

        yield return new WaitForSeconds(delayBetweenCards);

        while (CalculateHandValue(dealerHandIndices) < dealerThreshold)
        {
            yield return StartCoroutine(DrawCardRoutine(dealerHandContainer, dealerCards, dealerHandIndices, true, false));
            yield return new WaitForSeconds(delayBetweenCards);
        }

        int playerScore = CalculateHandValue(playerHandIndices);
        int dealerScore = CalculateHandValue(dealerHandIndices);

        if (dealerScore > 21)
        {
            Debug.Log("El Dealer se paso de 21. Jugador gana el derecho a disparar.");
        }
        else if (playerScore > dealerScore)
        {
            Debug.Log("Jugador gana la fase de cartas por puntos.");
        }
        else if (dealerScore > playerScore)
        {
            Debug.Log("Dealer gana la fase de cartas por puntos.");
        }
        else
        {
            Debug.Log("Empate en la fase de cartas.");
        }
    }

    public int CalculateHandValue(List<int> handIndices)
    {
        int totalValue = 0;
        int acesCount = 0;

        foreach (int index in handIndices)
        {
            CardData card = deckData[index];
            
            if (card.isAce)
            {
                acesCount++;
                totalValue += 11;
            }
            else
            {
                totalValue += card.cardValue;
            }
        }

        while (totalValue > 21 && acesCount > 0)
        {
            totalValue -= 10;
            acesCount--;
        }

        return totalValue;
    }
}