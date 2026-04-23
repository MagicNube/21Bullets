using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOpciones;
    public GameObject panelCreditos;
    public string nombreEscenaJuego;

    public AudioClip sonidoClick;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
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
                CerrarCreditos();
            }
        }
    }

    public void BotonJugar()
    {
        StartCoroutine(SecuenciaJugar());
    }

    IEnumerator SecuenciaJugar()
    {
        if (sonidoClick != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoClick);
        }

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void AbrirOpciones()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
            CanvasGroup cg = panelOpciones.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1;
        }
    }

    public void CerrarOpciones()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        StartCoroutine(RutinaFadeOut(panelOpciones));
    }

    public void AbrirCreditos()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        if (panelCreditos != null)
        {
            panelCreditos.SetActive(true);
            CanvasGroup cg = panelCreditos.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1;
        }
    }

    public void CerrarCreditos()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        StartCoroutine(RutinaFadeOut(panelCreditos));
    }

    private IEnumerator RutinaFadeOut(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            float tiempo = 0;
            float duracion = 0.5f;
            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                cg.alpha = Mathf.Lerp(1, 0, tiempo / duracion);
                yield return null;
            }
        }
        panel.SetActive(false);
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
