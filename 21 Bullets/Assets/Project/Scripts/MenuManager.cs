using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOpciones;
    public GameObject panelCreditos;
    public TextMeshProUGUI textoCabeceraCreditos;
    public string nombreEscenaJuego;

    public AudioClip sonidoClick;
    private AudioSource audioSource;

    // Esta variable guardará tus nombres y agradecimientos
    private string contenidoOriginalCreditos;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);

        // MUY IMPORTANTE: Guardamos el texto que escribiste en el Inspector al arrancar
        if (textoCabeceraCreditos != null)
        {
            contenidoOriginalCreditos = textoCabeceraCreditos.text;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelOpciones != null && panelOpciones.activeSelf)
            {
                CerrarOpciones();
            }
            else if (panelCreditos != null && panelCreditos.activeSelf)
            {
                if (SceneManager.GetActiveScene().name == nombreEscenaJuego)
                    ReiniciarPartida();
                else
                    CerrarCreditos();
            }
        }
    }

    public void AbrirCreditosResultado(bool haGanado)
    {
        if (panelCreditos != null)
        {
            panelCreditos.SetActive(true);

            if (textoCabeceraCreditos != null)
            {
                string titulo = haGanado ?
                    "<size=150%><b><color=#A30000>¡VICTORIA!</color></b></size>" :
                    "<size=150%><b><color=#555555>HAS CAÍDO...</color></b></size>";

                // Juntamos el título dinámico con tus créditos originales
                textoCabeceraCreditos.text = titulo + "\n\n" + contenidoOriginalCreditos;
            }

            CanvasGroup cg = panelCreditos.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1;
        }
    }

    public void AbrirCreditos()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);

        if (panelCreditos != null)
        {
            panelCreditos.SetActive(true);
            if (textoCabeceraCreditos != null)
            {
                // Juntamos el nombre del juego con tus créditos originales
                textoCabeceraCreditos.text = "<size=150%><b><color=#A30000>21 BULLETS</color></b></size>\n\n" + contenidoOriginalCreditos;
            }
        }
    }

    public void ReiniciarPartida()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BotonJugar()
    {
        StartCoroutine(SecuenciaJugar());
    }

    IEnumerator SecuenciaJugar()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void AbrirOpciones()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        if (panelOpciones != null) panelOpciones.SetActive(true);
    }

    public void CerrarOpciones()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        panelOpciones.SetActive(false);
    }

    public void CerrarCreditos()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        panelCreditos.SetActive(false);
    }

    public void BotonSalir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
