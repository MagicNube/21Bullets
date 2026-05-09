using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class SplashManager : MonoBehaviour
{
    public CanvasGroup panelSplashGroup;
    public RectTransform contenedorFlotante;
    public Animator animador21;
    public Animator animadorBullets;
    public RectTransform[] decoraciones;
    public CanvasGroup pantallazoFlash;
    public Camera camaraPrincipal;

    [Header("Escena a cargar")]
    public string nombreEscenaMenu = "01_Splash";

    [Header("Ajustes")]
    public AudioClip sonidoDisparo;
    public float delayEntreTextos = 0.8f;
    public float pausaAntesDisparo = 0.4f;
    public float tiempoAutoSkip = 4f;
    public float duracionFadeOut = 1f;

    private AudioSource audioSource;
    private Vector2 posInicialContenedor;
    private bool animacionIntroTerminada = false;
    private bool saltandoSplash = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (camaraPrincipal == null) camaraPrincipal = Camera.main;

        if (contenedorFlotante != null)
        {
            posInicialContenedor = contenedorFlotante.anchoredPosition;
        }

        for (int i = 0; i < decoraciones.Length; i++)
        {
            decoraciones[i].gameObject.SetActive(false);
        }
        pantallazoFlash.alpha = 0f;
        panelSplashGroup.alpha = 1f;

        StartCoroutine(SecuenciaSplash());
    }

    void Update()
    {
        if (animacionIntroTerminada && !saltandoSplash && contenedorFlotante != null)
        {
            float idleOffset = Mathf.Sin(Time.time * 2f) * 15f;
            contenedorFlotante.anchoredPosition = new Vector2(posInicialContenedor.x, posInicialContenedor.y + idleOffset);

            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                IniciarTransicion();
            }
        }
    }

    private IEnumerator SecuenciaSplash()
    {
        if (animador21 != null) animador21.Play("Entrada21");
        
        yield return new WaitForSeconds(delayEntreTextos);

        if (animadorBullets != null) animadorBullets.Play("EntradaBullets");

        yield return new WaitForSeconds(pausaAntesDisparo);

        if (sonidoDisparo != null) audioSource.PlayOneShot(sonidoDisparo);

        pantallazoFlash.alpha = 1f;
        for (int i = 0; i < decoraciones.Length; i++)
        {
            decoraciones[i].gameObject.SetActive(true);
        }
        StartCoroutine(RutinaSacudidaCamara(0.3f, 0.5f));

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            pantallazoFlash.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        pantallazoFlash.alpha = 0f;

        animacionIntroTerminada = true;

        yield return new WaitForSeconds(tiempoAutoSkip);

        if (!saltandoSplash)
        {
            IniciarTransicion();
        }
    }

    private IEnumerator RutinaSacudidaCamara(float duracion, float magnitud)
    {
        if (camaraPrincipal == null) yield break;

        Vector3 posOriginal = camaraPrincipal.transform.position;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            float x = Random.Range(-1f, 1f) * magnitud;
            float y = Random.Range(-1f, 1f) * magnitud;

            camaraPrincipal.transform.position = new Vector3(posOriginal.x + x, posOriginal.y + y, posOriginal.z);
            tiempo += Time.deltaTime;
            yield return null;
        }

        camaraPrincipal.transform.position = posOriginal;
    }

    private void IniciarTransicion()
    {
        saltandoSplash = true;
        StopAllCoroutines();
        StartCoroutine(RutinaTransicionMenu());
    }

    private IEnumerator RutinaTransicionMenu()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duracionFadeOut;
            panelSplashGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        panelSplashGroup.alpha = 0f;

        SceneManager.LoadScene(nombreEscenaMenu);
    }
}