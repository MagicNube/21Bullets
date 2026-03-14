using UnityEngine;
using UnityEngine.EventSystems;

public class HoverSonido : MonoBehaviour, IPointerEnterHandler
{
    public AudioClip sonidoHover;
    private AudioSource audioSource;

    void Start()
    {
        // Buscamos el AudioSource en el objeto MANAGERS para centralizar el audio
        audioSource = GameObject.Find("MANAGERS").GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("No se encontró AudioSource en el objeto MANAGERS");
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
