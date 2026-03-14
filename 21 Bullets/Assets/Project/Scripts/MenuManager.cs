using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOpciones;
    public string nombreEscenaJuego;

    public AudioClip sonidoClick; // Arrastra aquí tu sonido de "Roce" o "Click"
    private AudioSource audioSource;

    void Start()
    {
        // Buscamos o añadimos el AudioSource automáticamente
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }
    }

    public void BotonJugar()
    {
        // En lugar de cargar directo, lanzamos la secuencia
        StartCoroutine(SecuenciaJugar());
    }

    IEnumerator SecuenciaJugar()
    {
        // 1. Reproducir sonido
        if (sonidoClick != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoClick);
        }

        // 2. Esperar (por ejemplo, 0.5 segundos para que se oiga el sonido)
        // Si tienes un Fade Out, activa la animación aquí y espera lo que dure
        yield return new WaitForSeconds(0.5f);

        // 3. Cargar escena
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void AbrirOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(true);
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null) panelOpciones.SetActive(false);
    }

    public void BotonSalir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
