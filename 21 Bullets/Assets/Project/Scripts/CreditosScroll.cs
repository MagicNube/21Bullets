using UnityEngine;

public class CreditosScroll : MonoBehaviour
{
    public RectTransform textoCreditos;
    public float velocidadScroll = 100f;
    public float posicionInicialY = -1200f;

    void OnEnable()
    {
        if (textoCreditos != null)
        {
            Vector2 pos = textoCreditos.anchoredPosition;
            pos.y = posicionInicialY;
            textoCreditos.anchoredPosition = pos;
        }
    }

    void Update()
    {
        if (textoCreditos != null)
        {
            textoCreditos.anchoredPosition += Vector2.up * velocidadScroll * Time.deltaTime;
        }
    }
}
