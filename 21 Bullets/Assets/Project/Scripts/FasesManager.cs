using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FasesManager : MonoBehaviour
{
    [Header("Contenedores de Fase")]
    public GameObject contenedorCartas;
    public GameObject contenedorDisparo;

    [Header("Managers")]
    public CartasManager cartasManager;
    public DisparoManager disparoManager;
    public AnimacionTambor animacionTambor;
    public ObjetosManager objetosManager;
    public IntroAnimacionManager introManager;

    [Header("UI y Textos")]
    public TextMeshProUGUI textoCartas;
    public TextMeshProUGUI textoDisparo;

    [Header("Configuración Inicial (Versus)")]
    public Sprite spriteProtagonista;
    public Sprite spriteRivalInicial;

    private bool juegoIniciado = false;

    void Start()
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);
        textoCartas.text = "(Click para iniciar)";
    }

    void Update()
    {
        if (!juegoIniciado)
        {
            if (Input.GetMouseButtonDown(0))
            {
                juegoIniciado = true;
                IniciarFlujoPartida();
            }
        }
    }

    private void IniciarFlujoPartida()
    {
        textoCartas.text = "";

        if (introManager != null)
        {
            introManager.EjecutarAnimacion(spriteProtagonista, spriteRivalInicial);
        }
        else
        {
            IniciarFaseRecarga();
        }
    }

    public void IniciarFaseRecarga()
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);
        textoCartas.text = "";

        disparoManager.PrepararNuevaRonda();

        int numReales = disparoManager.ObtenerTotalRealesCalculadas();
        int numFogueo = disparoManager.ObtenerTotalFogueosCalculadas();

        animacionTambor.IniciarSecuencia(numReales, numFogueo, AlTerminarRecarga);
    }

    private void AlTerminarRecarga()
    {
        if (objetosManager != null)
        {
            objetosManager.RepartirObjetosRonda(IniciarFaseCartas);
        }
        else
        {
            IniciarFaseCartas();
        }
    }

    public void IniciarFaseCartas()
    {
        textoDisparo.text = "";
        contenedorDisparo.SetActive(false);
        contenedorCartas.SetActive(true);
        textoCartas.text = "Gana la mano para tener el primer turno.";
        cartasManager.ResetSequence();
    }

    public void IniciarFaseDisparo(bool ganaJugador)
    {
        textoCartas.text = "";
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(true);
        disparoManager.IniciarFase(ganaJugador);
    }

    public void ReiniciarRondaCompleta()
    {
        textoDisparo.text = "Tambor vacío. Siguiente ronda...";
        Invoke("IniciarFaseRecarga", 2f);
    }
}
