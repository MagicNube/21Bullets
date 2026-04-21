using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroAnimacionManager : MonoBehaviour
{
    public FasesManager fasesManager;

    [Header("Referencias de UI")]
    public GameObject panelIntro;
    public Image imagenJugador;
    public Image imagenRival;
    public TextMeshProUGUI textoNombreRival;
    public CanvasGroup canvasGroupPanel;

    [Header("Configuración de Animación")]
    public float tiempoFade = 1.0f;
    public float tiempoExposicion = 2.0f;
    public Vector3 posicionInicialJugador = new Vector3(-1000, 0, 0);
    public Vector3 posicionInicialRival = new Vector3(1000, 0, 0);

    private RectTransform rectJugador;
    private RectTransform rectRival;

    void Awake()
    {
        rectJugador = imagenJugador.GetComponent<RectTransform>();
        rectRival = imagenRival.GetComponent<RectTransform>();
        
        if (panelIntro != null) panelIntro.SetActive(false);
    }

    public void EjecutarAnimacionEntrada(Sprite spriteJugador, Sprite spriteRival, string nombreRival)
    {
        panelIntro.SetActive(true);
        imagenJugador.sprite = spriteJugador;
        imagenRival.sprite = spriteRival;
        textoNombreRival.text = "VS\n" + nombreRival;

        StartCoroutine(SecuenciaAnimacion());
    }

    private IEnumerator SecuenciaAnimacion()
    {
        canvasGroupPanel.alpha = 0;
        rectJugador.anchoredPosition = posicionInicialJugador;
        rectRival.anchoredPosition = posicionInicialRival;

        float tiempo = 0;
        while (tiempo < tiempoFade)
        {
            tiempo += Time.deltaTime;
            canvasGroupPanel.alpha = tiempo / tiempoFade;
            
            rectJugador.anchoredPosition = Vector3.Lerp(posicionInicialJugador, Vector2.zero, tiempo / tiempoFade);
            rectRival.anchoredPosition = Vector3.Lerp(posicionInicialRival, Vector2.zero, tiempo / tiempoFade);
            yield return null;
        }

        yield return new WaitForSeconds(tiempoExposicion);

        tiempo = 0;
        while (tiempo < tiempoFade)
        {
            tiempo += Time.deltaTime;
            canvasGroupPanel.alpha = 1 - (tiempo / tiempoFade);
            yield return null;
        }

        panelIntro.SetActive(false);
        
        fasesManager.IniciarFaseCartas();
    }
}