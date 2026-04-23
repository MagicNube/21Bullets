using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOpciones;
    public GameObject panelCreditos;
    public string nombreEscenaJuego;

    public AudioClip sonidoClick;
    public AudioMixerGroup grupoAudio;
    private AudioSource audioSource;

    IEnumerator Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (grupoAudio != null)
        {
            audioSource.outputAudioMixerGroup = grupoAudio;
        }

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }

        if (panelCreditos != null)
        {
            panelCreditos.SetActive(false);
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