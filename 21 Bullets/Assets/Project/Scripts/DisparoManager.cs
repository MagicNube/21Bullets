using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisparoManager : MonoBehaviour
{
    public FasesManager fasesManager;
    public DealerIANovato dealerIA;

    public Sprite spriteBalaReal;
    public Sprite spriteBalaFogueo;
    public Sprite spriteBalaDesconocida;
    public Sprite corazonLleno;
    public Sprite corazonVacio;

    public List<Image> slotsBalas;
    public Image[] corazonesJugador;
    public Image[] corazonesDealer;

    public TextMeshProUGUI textoReales;
    public TextMeshProUGUI textoFogueo;
    public TextMeshProUGUI textoPantalla;
    public Button botonYo;
    public Button botonDealer;

    public AudioClip sonidoDisparoReal;
    public AudioClip sonidoDisparoFogueo;

    private AudioSource audioSource;
    private List<bool> tambor = new List<bool>();
    private int indiceActual = 0;
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

    public void IniciarFase(bool turnoJugador)
    {
        volverAFaseCartas = false;
        turnoDelJugadorActual = turnoJugador;
        
        if (tambor.Count == 0 || indiceActual >= tambor.Count)
        {
            CargarTambor();
        }

        SetInteractuable(turnoDelJugadorActual);
        
        if (textoPantalla != null)
        {
            textoPantalla.text = turnoDelJugadorActual ? "TU TURNO: Elige objetivo" : "TURNO DEL DEALER (Espera...)";
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
            else if (volverAFaseCartas)
            {
                fasesManager.IniciarFaseCartas();
            }
            else
            {
                if (turnoDelJugadorActual)
                {
                    SetInteractuable(true);
                    if (textoPantalla != null) textoPantalla.text = "TU TURNO: Elige objetivo";
                }
                else
                {
                    SetInteractuable(false);
                    if (textoPantalla != null) textoPantalla.text = "TURNO DEL DEALER (Espera...)";
                    if (dealerIA != null) dealerIA.IniciarTurno();
                }
            }
        }
    }

    public void CargarTambor()
    {
        indiceActual = 0;
        tambor = new List<bool>();

        int numReales = Random.Range(2, 5);
        int numFogueo = 6 - numReales;

        for (int i = 0; i < numReales; i++) tambor.Add(true);
        for (int i = 0; i < numFogueo; i++) tambor.Add(false);

        for (int i = 0; i < tambor.Count; i++)
        {
            bool temp = tambor[i];
            int randomIndex = Random.Range(i, tambor.Count);
            tambor[i] = tambor[randomIndex];
            tambor[randomIndex] = temp;
        }

        foreach (Image slot in slotsBalas)
        {
            if (slot != null)
            {
                slot.sprite = spriteBalaDesconocida;
                slot.color = Color.white;
            }
        }

        ActualizarContadoresTexto(numReales, numFogueo);
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
        if (textoPantalla != null)
        {
            textoPantalla.text = dispararAJugador ? "El Dealer te apunta a TI..." : "El Dealer se apunta a SI MISMO...";
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
                FinalizarTurno(tamborVacio ? "¡BALA REAL! El Dealer te ha dañado. Tambor vacio." : "¡BALA REAL! El Dealer te ha dañado. Volviendo a las cartas...", true);
            }
            else
            {
                FinalizarTurno(tamborVacio ? "¡FOGUEO! El Dealer te disparo y fallo. Tambor vacio." : "¡FOGUEO! El Dealer te disparo y fallo. Volviendo a las cartas...", true);
            }
        }
        else
        {
            if (esReal)
            {
                vidasDealer--;
                ActualizarCorazonesUI();
                FinalizarTurno(tamborVacio ? "¡BALA REAL! El Dealer se ha dañado a si mismo. Tambor vacio." : "¡BALA REAL! El Dealer se ha dañado. Volviendo a las cartas...", true);
            }
            else
            {
                turnoDelJugadorActual = false;
                FinalizarTurno(tamborVacio ? "¡FOGUEO! El Dealer se disparo y se salva. Tambor vacio." : "¡FOGUEO! El Dealer se salva y MANTIENE EL TURNO.", tamborVacio);
            }
        }
    }

    public void DispararAYo()
    {
        if (!ComprobarSeguridad()) return;

        bool esReal = EjecutarDisparoLogico();
        bool tamborVacio = (indiceActual >= tambor.Count);

        if (esReal)
        {
            vidasJugador--;
            ActualizarCorazonesUI();
            FinalizarTurno(tamborVacio ? "¡BALA REAL! Te has dañado. Tambor vacio." : "¡BALA REAL! Te has dañado. Volviendo a las cartas...", true);
        }
        else
        {
            turnoDelJugadorActual = true;
            FinalizarTurno(tamborVacio ? "¡FOGUEO! Te salvaste. Tambor vacio." : "¡FOGUEO! Te salvaste. Mantienes el turno.", tamborVacio);
        }
    }

    public void DispararADealer()
    {
        if (!ComprobarSeguridad()) return;

        bool esReal = EjecutarDisparoLogico();
        bool tamborVacio = (indiceActual >= tambor.Count);

        if (esReal)
        {
            vidasDealer--;
            ActualizarCorazonesUI();
            FinalizarTurno(tamborVacio ? "¡IMPACTO! El Dealer recibe daño. Tambor vacio." : "¡IMPACTO! El Dealer recibe daño. Volviendo a las cartas...", true);
        }
        else
        {
            FinalizarTurno(tamborVacio ? "¡FOGUEO! El Dealer se salva. Tambor vacio." : "¡FOGUEO! El Dealer se salva. Volviendo a las cartas...", true);
        }
    }

    private bool ComprobarSeguridad()
    {
        if (tambor == null || tambor.Count == 0 || indiceActual >= tambor.Count)
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
        return esReal;
    }

    private void FinalizarTurno(string mensaje, bool volverACartas)
    {
        volverAFaseCartas = volverACartas;
        if (textoPantalla != null) textoPantalla.text = mensaje + "\n(Click para continuar)";
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