using UnityEngine;
using TMPro;

public class CreditosScroll : MonoBehaviour
{
    public RectTransform textoCreditos;
    public float velocidadScroll = 100f;
    public float posicionInicialY = -1200f;

    [Header("Configuracion Salida")]
    public GameObject textoEscape;
    public float segundosParaMostrar = 20f;
    private float cronometro = 0f;

    void OnEnable()
    {
        if (textoCreditos != null)
        {
            Vector2 pos = textoCreditos.anchoredPosition;
            pos.y = posicionInicialY;
            textoCreditos.anchoredPosition = pos;
        }

        if (textoEscape != null)
        {
            textoEscape.SetActive(false);
        }

        cronometro = 0f;
    }

    void Update()
    {
        if (textoCreditos != null)
        {
            textoCreditos.anchoredPosition += Vector2.up * velocidadScroll * Time.deltaTime;
        }

        if (textoEscape != null && !textoEscape.activeSelf)
        {
            cronometro += Time.deltaTime;
            if (cronometro >= segundosParaMostrar)
            {
                textoEscape.SetActive(true);
            }
        }
    }
}
