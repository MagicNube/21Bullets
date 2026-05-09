using UnityEngine;
using UnityEngine.EventSystems;

public class HoverSonido : MonoBehaviour, IPointerEnterHandler
{
    public AudioClip sonidoHover;
    private AudioSource audioSource;

    void Start()
    {
        MenuManager menuManager = Object.FindAnyObjectByType<MenuManager>();
        if (menuManager != null) 
        {
            audioSource = menuManager.GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            GameplayMenuManager gameplayManager = Object.FindAnyObjectByType<GameplayMenuManager>();
            if (gameplayManager != null) 
            {
                audioSource = gameplayManager.GetComponent<AudioSource>();
            }
        }

        if (audioSource == null)
        {
            GameObject managersObj = GameObject.Find("MANAGERS");
            if (managersObj != null) 
            {
                audioSource = managersObj.GetComponent<AudioSource>();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (sonidoHover != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoHover);
        }
    }
}