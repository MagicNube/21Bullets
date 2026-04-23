using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroAnimacionManager : MonoBehaviour
{
    public FasesManager fasesManager;

    [Header("Referencias UI")]
    public GameObject objetoRaiz;
    public CanvasGroup grupoCanvas;
    public RectTransform transJugador;
    public RectTransform transRival;
    public Image imgJugador;
    public Image imgRival;
    public GameObject objetoTextoVS;

    [Header("Ajustes")]
    public float tiempoFade = 0.8f;
    public float esperaCentro = 2.0f;
    public float distanciaEntrada = 1000f;

    private Vector2 posOriginalJugador;
    private Vector2 posOriginalRival;

    void Awake()
    {
        posOriginalJugador = transJugador.anchoredPosition;
        posOriginalRival = transRival.anchoredPosition;

        if (objetoRaiz != null) objetoRaiz.SetActive(false);
    }

    public void EjecutarAnimacion(Sprite prota, Sprite rival)
    {
        if (fasesManager != null)
        {
            if (fasesManager.panelHUDJugador != null) fasesManager.panelHUDJugador.SetActive(false);
            if (fasesManager.panelHUDDealer != null) fasesManager.panelHUDDealer.SetActive(false);
            if (fasesManager.botonVersus1 != null) fasesManager.botonVersus1.gameObject.SetActive(false);
            if (fasesManager.botonVersus2 != null) fasesManager.botonVersus2.gameObject.SetActive(false);
        }

        objetoRaiz.SetActive(true);

        if (objetoTextoVS != null) objetoTextoVS.SetActive(true);

        imgJugador.sprite = prota;
        imgRival.sprite = rival;

        StartCoroutine(RutinaAnimacion());
    }

    private IEnumerator RutinaAnimacion()
    {
        grupoCanvas.alpha = 0;
        transJugador.anchoredPosition = posOriginalJugador + new Vector2(-distanciaEntrada, 0);
        transRival.anchoredPosition = posOriginalRival + new Vector2(distanciaEntrada, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / tiempoFade;
            float easedT = t * t * (3f - 2f * t);

            grupoCanvas.alpha = t;
            transJugador.anchoredPosition = Vector2.Lerp(posOriginalJugador + new Vector2(-distanciaEntrada, 0), posOriginalJugador, easedT);
            transRival.anchoredPosition = Vector2.Lerp(posOriginalRival + new Vector2(distanciaEntrada, 0), posOriginalRival, easedT);
            yield return null;
        }

        yield return new WaitForSeconds(esperaCentro);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / tiempoFade;
            grupoCanvas.alpha = 1 - t;
            yield return null;
        }

        objetoRaiz.SetActive(false);
        fasesManager.IniciarFaseRecarga();
    }
}
