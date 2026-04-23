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

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        if (panelCreditos != null)
        {
            panelCreditos.SetActive(false);
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
        if (panelOpciones != null) panelOpciones.SetActive(true);
    }

    public void CerrarOpciones()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void AbrirCreditos()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        if (panelCreditos != null) panelCreditos.SetActive(true);
    }

    public void CerrarCreditos()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        if (panelCreditos != null) panelCreditos.SetActive(false);
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
