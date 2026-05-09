using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using TMPro;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOpciones;
    public GameObject panelCreditos;
    public GameObject panelDificultad;
    public TextMeshProUGUI textoCabeceraCreditos;

    [Header("Configuracion de Escenas")]
    public string nombreEscenaJuego;
    public string nombreEscenaMenu;

    public AudioClip sonidoClick;
    public AudioMixerGroup grupoAudio;
    private AudioSource audioSource;

    private string contenidoOriginalCreditos;

    IEnumerator Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (grupoAudio != null)
        {
            audioSource.outputAudioMixerGroup = grupoAudio;
        }

        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelDificultad != null) panelDificultad.SetActive(false);

        if (textoCabeceraCreditos != null)
        {
            contenidoOriginalCreditos = textoCabeceraCreditos.text;
        }

        yield return null;

        if (grupoAudio != null)
        {
            float volumenGuardado = PlayerPrefs.GetFloat("VolumenMaster", 0.75f);
            float volumenEnDecibelios = Mathf.Log10(Mathf.Clamp(volumenGuardado, 0.0001f, 1f)) * 20f;
            grupoAudio.audioMixer.SetFloat("VolumenMaster", volumenEnDecibelios);
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
            else if (panelDificultad != null && panelDificultad.activeSelf)
            {
                CerrarDificultad();
            }
            else if (panelCreditos != null && panelCreditos.activeSelf)
            {
                if (SceneManager.GetActiveScene().name == nombreEscenaJuego)
                {
                    VolverAlMenu();
                }
                else
                {
                    CerrarCreditos();
                }
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

                textoCabeceraCreditos.text = titulo + "\n\n" + contenidoOriginalCreditos;
            }

            CanvasGroup cg = panelCreditos.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1;
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void AbrirCreditos()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);

        if (panelCreditos != null)
        {
            panelCreditos.SetActive(true);
            if (textoCabeceraCreditos != null)
            {
                textoCabeceraCreditos.text = "<size=150%><b><color=#A30000>21 BULLETS</color></b></size>\n\n" + contenidoOriginalCreditos;
            }
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void VolverAlMenu()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);

        if (!string.IsNullOrEmpty(nombreEscenaMenu))
        {
            SceneManager.LoadScene(nombreEscenaMenu);
        }
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void AbrirDificultad()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        if (panelDificultad != null) panelDificultad.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CerrarDificultad()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        if (panelDificultad != null) panelDificultad.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ConfirmarJuego()
    {
        StartCoroutine(SecuenciaJugar());
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void BotonJugar()
    {
        AbrirDificultad();
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
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CerrarOpciones()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        if (panelOpciones != null) panelOpciones.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CerrarCreditos()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
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