using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisparoManager : MonoBehaviour
{
    public FasesManager fasesManager;

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

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        vidasJugador = 3;
        vidasDealer = 3;
        ActualizarCorazonesUI();

        if (botonYo != null) botonYo.onClick.AddListener(DispararAYo);
        if (botonDealer != null) botonDealer.onClick.AddListener(DispararADealer);
    }

    public void IniciarFase(bool turnoJugador)
    {
        volverAFaseCartas = false;
        
        if (tambor.Count == 0 || indiceActual >= tambor.Count)
        {
            CargarTambor();
        }

        SetInteractuable(turnoJugador);
        
        if (textoPantalla != null)
        {
            textoPantalla.text = turnoJugador ? "TU TURNO: Elige objetivo" : "TURNO DEL DEALER (Espera...)";
        }

        // Si no es tu turno, arranca el cerebro básico del Dealer
        if (!turnoJugador)
        {
            StartCoroutine(TurnoDealerRoutine());
        }
    }

    private IEnumerator TurnoDealerRoutine()
    {
        // El Dealer espera 1.5 segundos para darle tensión al momento
        yield return new WaitForSeconds(1.5f);

        if (!ComprobarSeguridad()) yield break;

        // El Dealer decide dispararte a ti siempre (por ahora)
        bool esReal = EjecutarDisparoLogico();

        if (esReal)
        {
            vidasJugador--;
            ActualizarCorazonesUI();
            FinalizarTurno("¡El Dealer te ha disparado con una BALA REAL!", true);
        }
        else
        {
            if (indiceActual >= tambor.Count)
            {
                FinalizarTurno("¡FOGUEO! Te salvaste. Tambor vacio.", true);
            }
            else
            {
                FinalizarTurno("¡FOGUEO! El Dealer te disparo pero te salvaste.", true);
            }
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
                SetInteractuable(true);
                if (textoPantalla != null) textoPantalla.text = "FASE DE DISPARO: Elige objetivo";
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

    public void DispararAYo()
    {
        if (!ComprobarSeguridad()) return;

        bool esReal = EjecutarDisparoLogico();

        if (esReal)
        {
            vidasJugador--;
            ActualizarCorazonesUI();
            FinalizarTurno("¡BALA REAL! Te has dañado.", true);
        }
        else
        {
            if (indiceActual >= tambor.Count)
            {
                FinalizarTurno("¡FOGUEO! Tambor vacio.", true);
            }
            else
            {
                if (textoPantalla != null) textoPantalla.text = "¡FOGUEO! Mantienes el turno. Dispara otra vez.";
                SetInteractuable(true);
                turnoTerminado = false;
            }
        }
    }

    public void DispararADealer()
    {
        if (!ComprobarSeguridad()) return;

        bool esReal = EjecutarDisparoLogico();

        if (esReal)
        {
            vidasDealer--;
            ActualizarCorazonesUI();
            FinalizarTurno("¡IMPACTO! El Dealer recibe daño.", true);
        }
        else
        {
            if (indiceActual >= tambor.Count)
            {
                FinalizarTurno("¡FOGUEO! El Dealer se salva. Tambor vacio.", true);
            }
            else
            {
                FinalizarTurno("¡FOGUEO! El Dealer se salva. Pierdes el turno.", true);
            }
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