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

public class FaseCartas : MonoBehaviour
{
    public List<CardData> deckData;
    public Sprite cardBackSprite;
    public GameObject cardPrefab;
    
    public Transform playerHandContainer;
    public Transform dealerHandContainer;
    public Transform deckTransform;

    public Button hitButton;
    public Button standButton;
    public int dealerThreshold = 14;

    public float cardSpacing = 1.5f;
    public float cardMoveDuration = 0.4f;
    public float delayBetweenCards = 0.2f;
    public float flipDuration = 0.3f;

    private List<int> deck = new List<int>();
    private List<GameObject> playerCards = new List<GameObject>();
    private List<GameObject> dealerCards = new List<GameObject>();

    private List<int> playerHandIndices = new List<int>();
    private List<int> dealerHandIndices = new List<int>();

    private bool isPlayerTurn = false;
    private SpriteRenderer dealerHiddenCardRenderer;
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
            if (resetTimer >= 5f)
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
        dealerHiddenCardRenderer = null;
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

    public IEnumerator DrawCardRoutine(Transform container, List<GameObject> handList, List<int> indexList, bool isVisible, bool isHiddenDealerCard)
    {
        if (deck.Count == 0) yield break;

        int cardIndex = deck[0];
        deck.RemoveAt(0);
        indexList.Add(cardIndex);

        GameObject newCard = Instantiate(cardPrefab, container);
        Transform cardTransform = newCard.transform;

        handList.Add(newCard);
        int totalCards = handList.Count;

        float totalWidth = (totalCards - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        Vector3[] startPositions = new Vector3[totalCards - 1];
        Vector3[] targetPositions = new Vector3[totalCards];

        for (int i = 0; i < totalCards; i++)
        {
            targetPositions[i] = new Vector3(startX + (i * cardSpacing), 0, 0);
            if (i < totalCards - 1)
            {
                startPositions[i] = handList[i].transform.localPosition;
            }
        }

        cardTransform.localPosition = targetPositions[totalCards - 1];
        Vector3 finalWorldPosition = cardTransform.position;
        Vector3 startWorldPosition = deckTransform.position;
        cardTransform.position = startWorldPosition;

        SpriteRenderer cardRenderer = newCard.GetComponent<SpriteRenderer>();
        
        if (cardRenderer != null)
        {
            cardRenderer.sprite = cardBackSprite;
            if (isHiddenDealerCard)
            {
                dealerHiddenCardRenderer = cardRenderer;
                dealerHiddenCardIndex = cardIndex;
            }
        }

        float elapsedTime = 0f;
        
        while (elapsedTime < cardMoveDuration)
        {
            float t = elapsedTime / cardMoveDuration;
            float easeOutCubic = 1f - Mathf.Pow(1f - t, 3f);

            for (int i = 0; i < totalCards - 1; i++)
            {
                Transform existingTransform = handList[i].transform;
                existingTransform.localPosition = Vector3.Lerp(startPositions[i], targetPositions[i], easeOutCubic);
            }
            
            cardTransform.position = Vector3.Lerp(startWorldPosition, finalWorldPosition, easeOutCubic);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < totalCards; i++)
        {
            handList[i].transform.localPosition = targetPositions[i];
        }

        if (isVisible && cardRenderer != null)
        {
            yield return StartCoroutine(FlipCardRoutine(cardTransform, cardRenderer, deckData[cardIndex].cardSprite));
        }
    }

    public IEnumerator FlipCardRoutine(Transform cardTransform, SpriteRenderer cardRenderer, Sprite targetSprite)
    {
        float halfDuration = flipDuration / 2f;
        float elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            float smoothStep = t * t * (3f - 2f * t);
            
            float scaleX = Mathf.Lerp(1f, 0f, smoothStep);
            cardTransform.localScale = new Vector3(scaleX * cardPrefab.transform.localScale.x, cardPrefab.transform.localScale.y, cardPrefab.transform.localScale.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cardTransform.localScale = new Vector3(0f, cardPrefab.transform.localScale.y, cardPrefab.transform.localScale.z);
        cardRenderer.sprite = targetSprite;

        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            float smoothStep = t * t * (3f - 2f * t);
            
            float scaleX = Mathf.Lerp(0f, 1f, smoothStep);
            cardTransform.localScale = new Vector3(scaleX * cardPrefab.transform.localScale.x, cardPrefab.transform.localScale.y, cardPrefab.transform.localScale.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cardTransform.localScale = cardPrefab.transform.localScale;
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
        if (dealerHiddenCardRenderer != null)
        {
            yield return StartCoroutine(FlipCardRoutine(dealerHiddenCardRenderer.transform, dealerHiddenCardRenderer, deckData[dealerHiddenCardIndex].cardSprite));
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