using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Audio;
using System.Collections;

public class GameplayMenuManager : MonoBehaviour
{
    public GameObject panelPausa;
    public GameObject panelOpciones;
    public GameObject panelCreditosFinal;
    public string nombreEscenaMenu;

    public AudioClip sonidoClick;
    public AudioMixerGroup grupoAudio;
    private AudioSource audioSource;

    private bool estaPausado = false;

    IEnumerator Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (grupoAudio != null)
        {
            audioSource.outputAudioMixerGroup = grupoAudio;
        }

        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);

        Time.timeScale = 1f;

        yield return null;

        if (grupoAudio != null)
        {
            CargarVolumenGuardado("VolumenMaster", 0.75f);
            CargarVolumenGuardado("VolumenMusica", 0.75f);
            CargarVolumenGuardado("VolumenSFX", 0.75f);
        }
    }

    private void CargarVolumenGuardado(string parametro, float valorPorDefecto)
    {
        float volumenGuardado = PlayerPrefs.GetFloat(parametro, valorPorDefecto);
        float volumenEnDecibelios = Mathf.Log10(Mathf.Clamp(volumenGuardado, 0.0001f, 1f)) * 20f;
        grupoAudio.audioMixer.SetFloat(parametro, volumenEnDecibelios);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelCreditosFinal != null && panelCreditosFinal.activeSelf)
            {
                return;
            }

            if (panelOpciones != null && panelOpciones.activeSelf)
            {
                CerrarOpciones();
            }
            else
            {
                TogglePausa();
            }
        }
    }

    public void TogglePausa()
    {
        estaPausado = !estaPausado;
        if (panelPausa != null) panelPausa.SetActive(estaPausado);
        Time.timeScale = estaPausado ? 0f : 1f;
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
    }

    public void Continuar()
    {
        estaPausado = false;
        if (panelPausa != null) panelPausa.SetActive(false);
        Time.timeScale = 1f;
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
    }

    public void AbrirOpciones()
    {
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(true);
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelPausa != null) panelPausa.SetActive(true);
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
    }

    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        StartCoroutine(SecuenciaCambioEscena(nombreEscenaMenu));
    }

    public void SalirDelJuego()
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator SecuenciaCambioEscena(string escena)
    {
        if (sonidoClick != null && audioSource != null) audioSource.PlayOneShot(sonidoClick);
        yield return new WaitForSecondsRealtime(0.4f);
        SceneManager.LoadScene(escena);
    }
}