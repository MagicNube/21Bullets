using System.Collections;
using UnityEngine;
using TMPro;

public class NotificacionManager : MonoBehaviour
{
    public static NotificacionManager Instance;

    public RectTransform panelNotificacion;
    public TextMeshProUGUI textoNotificacion;

    public float posicionVisibleY = -50f;
    public float posicionOcultaY = 150f;
    public float velocidadAnimacion = 0.5f;

    private Coroutine rutinaActual;

    void Awake()
    {
        if (Instance == null) Instance = this;

        if (panelNotificacion != null)
        {
            panelNotificacion.anchoredPosition = new Vector2(panelNotificacion.anchoredPosition.x, posicionOcultaY);
        }
    }

    public void MostrarMensaje(string mensaje, bool autoOcultar = false, float tiempoVisible = 2f)
    {
        if (rutinaActual != null) StopCoroutine(rutinaActual);
        rutinaActual = StartCoroutine(RutinaAnimacion(mensaje, autoOcultar, tiempoVisible));
    }

    public void OcultarMensaje()
    {
        if (rutinaActual != null) StopCoroutine(rutinaActual);
        rutinaActual = StartCoroutine(RutinaOcultar());
    }

    private IEnumerator RutinaAnimacion(string mensaje, bool autoOcultar, float tiempoVisible)
    {
        textoNotificacion.text = mensaje;

        float t = 0;
        Vector2 posInicial = panelNotificacion.anchoredPosition;
        Vector2 posFinal = new Vector2(posInicial.x, posicionVisibleY);

        while (t < 1)
        {
            t += Time.deltaTime / velocidadAnimacion;
            float easedT = t * t * (3f - 2f * t);
            panelNotificacion.anchoredPosition = Vector2.Lerp(posInicial, posFinal, easedT);
            yield return null;
        }

        panelNotificacion.anchoredPosition = posFinal;

        if (autoOcultar)
        {
            yield return new WaitForSeconds(tiempoVisible);
            yield return StartCoroutine(RutinaOcultar());
        }
    }

    private IEnumerator RutinaOcultar()
    {
        float t = 0;
        Vector2 posInicial = panelNotificacion.anchoredPosition;
        Vector2 posFinal = new Vector2(posInicial.x, posicionOcultaY);

        while (t < 1)
        {
            t += Time.deltaTime / velocidadAnimacion;
            float easedT = t * t * (3f - 2f * t);
            panelNotificacion.anchoredPosition = Vector2.Lerp(posInicial, posFinal, easedT);
            yield return null;
        }

        panelNotificacion.anchoredPosition = posFinal;
    }
}