using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisparoManager : MonoBehaviour
{
    public FasesManager fasesManager;
    public DealerIANovato dealerIA;
    public EstacionBalasControl estacionBalas;

    [Header("Visuales Pistola")]
    public Image component_PistolaMesa;
    public Sprite vista_Mesa;
    public Sprite vista_AlDealer;
    public Sprite vista_AlJugador;

    public Sprite spriteBalaReal;
    public Sprite spriteBalaFogueo;
    public Sprite spriteBalaDesconocida;
    public Sprite corazonLleno;
    public Sprite corazonVacio;

    public List<SpriteRenderer> slotsBalas;
    public Image[] corazonesJugador;
    public Image[] corazonesDealer;

    public TMP_Text textoReales;
    public TMP_Text textoFogueo;
    public TMP_Text textoPantalla;
    public Button botonYo;
    public Button botonDealer;

    public AudioClip sonidoDisparoReal;
    public AudioClip sonidoDisparoFogueo;

    private AudioSource audioSource;
    private List<bool> tambor = new List<bool>();
    private int indiceActual = 0;

    private int totalRealesCalculadas = 0;
    private int totalFogueosCalculadas = 0;

    private int vidasJugador = 3;
    private int vidasDealer = 3;
    private bool turnoTerminado = false;
    private bool volverAFaseCartas = false;
    private bool turnoDelJugadorActual = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        vidasJugador = 3;
        vidasDealer = 3;
        ActualizarCorazonesUI();

        if (component_PistolaMesa != null)
        {
            component_PistolaMesa.gameObject.SetActive(false);
        }

        if (botonYo != null)
        {
            botonYo.onClick.RemoveAllListeners();
            botonYo.onClick.AddListener(DispararAYo);
        }

        if (botonDealer != null)
        {
            botonDealer.onClick.RemoveAllListeners();
            botonDealer.onClick.AddListener(DispararADealer);
        }
    }

    public void PrepararNuevaRonda()
    {
        indiceActual = 0;
        tambor = new List<bool>();

        totalRealesCalculadas = UnityEngine.Random.Range(2, 5);
        totalFogueosCalculadas = 6 - totalRealesCalculadas;

        for (int i = 0; i < totalRealesCalculadas; i++) tambor.Add(true);
        for (int i = 0; i < totalFogueosCalculadas; i++) tambor.Add(false);

        for (int i = 0; i < tambor.Count; i++)
        {
            bool temp = tambor[i];
            int randomIndex = UnityEngine.Random.Range(i, tambor.Count);
            tambor[i] = tambor[randomIndex];
            tambor[randomIndex] = temp;
        }

        foreach (SpriteRenderer slot in slotsBalas)
        {
            if (slot != null)
            {
                slot.sprite = spriteBalaDesconocida;
                slot.color = Color.white;
            }
        }

        ActualizarContadoresTexto(ObtenerRealesRestantes(), ObtenerFogueosRestantes());
    }

    public int ObtenerTotalRealesCalculadas() { return totalRealesCalculadas; }
    public int ObtenerTotalFogueosCalculadas() { return totalFogueosCalculadas; }

    public void IniciarFase(bool turnoJugador)
    {
        volverAFaseCartas = false;
        turnoDelJugadorActual = turnoJugador;

        ActualizarContadoresTexto(ObtenerRealesRestantes(), ObtenerFogueosRestantes());

        if (component_PistolaMesa != null)
        {
            component_PistolaMesa.gameObject.SetActive(true);
            component_PistolaMesa.sprite = vista_Mesa;
        }

        if (estacionBalas != null)
        {
            estacionBalas.Desplegar();
        }

        SetInteractuable(turnoDelJugadorActual);

        if (textoPantalla != null)
        {
            textoPantalla.text = turnoDelJugadorActual ? "TU TURNO" : "TURNO DEL DEALER";
        }

        if (!turnoDelJugadorActual && dealerIA != null)
        {
            dealerIA.IniciarTurno();
        }
    }

    void Update()
    {
        if (turnoTerminado && Input.GetMouseButtonDown(0))
        {
            turnoTerminado = false;

            if (vidasJugador <= 0 || vidasDealer <= 0)
            {
                if (textoPantalla != null) textoPantalla.text = "FIN DE LA PARTIDA";
            }
            else if (indiceActual >= tambor.Count)
            {
                if (component_PistolaMesa != null) component_PistolaMesa.gameObject.SetActive(false);
                if (estacionBalas != null) estacionBalas.Ocultar();
                fasesManager.ReiniciarRondaCompleta();
            }
            else if (volverAFaseCartas)
            {
                if (component_PistolaMesa != null) component_PistolaMesa.gameObject.SetActive(false);
                if (estacionBalas != null) estacionBalas.Ocultar();
                fasesManager.IniciarFaseCartas();
            }
            else
            {
                if (component_PistolaMesa != null) component_PistolaMesa.sprite = vista_Mesa;

                if (turnoDelJugadorActual)
                {
                    SetInteractuable(true);
                    if (textoPantalla != null) textoPantalla.text = "TU TURNO";
                }
                else
                {
                    SetInteractuable(false);
                    if (textoPantalla != null) textoPantalla.text = "TURNO DEL DEALER";
                    if (dealerIA != null) dealerIA.IniciarTurno();
                }
            }
        }
    }

    public int ObtenerRealesRestantes()
    {
        int count = 0;
        for (int i = indiceActual; i < tambor.Count; i++)
        {
            if (tambor[i]) count++;
        }
        return count;
    }

    public int ObtenerFogueosRestantes()
    {
        int count = 0;
        for (int i = indiceActual; i < tambor.Count; i++)
        {
            if (!tambor[i]) count++;
        }
        return count;
    }

    public void RecibirDecisionDealer(bool dispararAJugador)
    {
        StartCoroutine(EjecutarDisparoDealerRoutine(dispararAJugador));
    }

    private IEnumerator EjecutarDisparoDealerRoutine(bool dispararAJugador)
    {
        if (component_PistolaMesa != null)
        {
            component_PistolaMesa.sprite = dispararAJugador ? vista_AlJugador : vista_AlDealer;
        }

        yield return new WaitForSeconds(1.5f);

        if (!ComprobarSeguridad()) yield break;

        bool esReal = EjecutarDisparoLogico();
        bool tamborVacio = (indiceActual >= tambor.Count);

        if (dispararAJugador)
        {
            if (esReal)
            {
                vidasJugador--;
                ActualizarCorazonesUI();
                FinalizarTurno(tamborVacio ? "¡BALA REAL" : "¡BALA REAL!", true);
            }
            else
            {
                FinalizarTurno(tamborVacio ? "¡FOGUEO!" : "¡FOGUEO!", true);
            }
        }
        else
        {
            if (esReal)
            {
                vidasDealer--;
                ActualizarCorazonesUI();
                FinalizarTurno(tamborVacio ? "" : "", true);
            }
            else
            {
                turnoDelJugadorActual = false;
                FinalizarTurno(tamborVacio ? "" : "", false);
            }
        }
    }

    public void DispararAYo()
    {
        if (!ComprobarSeguridad()) return;
        SetInteractuable(false);
        StartCoroutine(RutinaDisparoJugador(true));
    }

    public void DispararADealer()
    {
        if (!ComprobarSeguridad()) return;
        SetInteractuable(false);
        StartCoroutine(RutinaDisparoJugador(false));
    }

    private IEnumerator RutinaDisparoJugador(bool dispararAJugador)
    {
        if (component_PistolaMesa != null)
        {
            component_PistolaMesa.sprite = dispararAJugador ? vista_AlJugador : vista_AlDealer;
        }

        yield return new WaitForSeconds(0.6f);

        bool esReal = EjecutarDisparoLogico();
        bool tamborVacio = (indiceActual >= tambor.Count);

        if (dispararAJugador)
        {
            if (esReal)
            {
                vidasJugador--;
                ActualizarCorazonesUI();
                FinalizarTurno(tamborVacio ? "" : "", true);
            }
            else
            {
                turnoDelJugadorActual = true;
                FinalizarTurno(tamborVacio ? "" : "", false);
            }
        }
        else
        {
            if (esReal)
            {
                vidasDealer--;
                ActualizarCorazonesUI();
                FinalizarTurno(tamborVacio ? "" : "", true);
            }
            else
            {
                FinalizarTurno(tamborVacio ? "" : "", true);
            }
        }
    }

    private bool ComprobarSeguridad()
    {
        if (tambor == null || tambor.Count == 0 || indiceActual > tambor.Count)
        {
            return false;
        }
        return true;
    }

    private bool EjecutarDisparoLogico()
    {
        bool esReal = tambor[indiceActual];

        if (slotsBalas[indiceActual] != null)
        {
            slotsBalas[indiceActual].sprite = esReal ? spriteBalaReal : spriteBalaFogueo;
            slotsBalas[indiceActual].color = Color.white;
        }

        if (audioSource != null && (esReal ? sonidoDisparoReal : sonidoDisparoFogueo) != null)
        {
            audioSource.PlayOneShot(esReal ? sonidoDisparoReal : sonidoDisparoFogueo);
        }

        indiceActual++;
        ActualizarContadoresTexto(ObtenerRealesRestantes(), ObtenerFogueosRestantes());

        return esReal;
    }

    private void FinalizarTurno(string mensaje, bool volverACartas)
    {
        volverAFaseCartas = volverACartas;
        if (textoPantalla != null) textoPantalla.text = mensaje + "";
        SetInteractuable(false);
        turnoTerminado = true;
    }

    private void ActualizarCorazonesUI()
    {
        for (int i = 0; i < corazonesJugador.Length; i++)
        {
            if (corazonesJugador[i] != null)
                corazonesJugador[i].sprite = (i < vidasJugador) ? corazonLleno : corazonVacio;
        }

        for (int i = 0; i < corazonesDealer.Length; i++)
        {
            if (corazonesDealer[i] != null)
                corazonesDealer[i].sprite = (i < vidasDealer) ? corazonLleno : corazonVacio;
        }
    }

    private void ActualizarContadoresTexto(int r, int f)
    {
        if (textoReales != null) textoReales.text = r + "x Reales";
        if (textoFogueo != null) textoFogueo.text = f + "x Fogueo";
    }

    private void SetInteractuable(bool estado)
    {
        if (botonYo != null) botonYo.interactable = estado;
        if (botonDealer != null) botonDealer.interactable = estado;
    }
}
