using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FasesManager : MonoBehaviour
{
    public GameObject contenedorCartas;
    public GameObject contenedorDisparo;

    public CartasManager cartasManager;
    public DisparoManager disparoManager;
    public AnimacionTambor animacionTambor;
    public ObjetosManager objetosManager;

    public TextMeshProUGUI textoCartas;
    public TextMeshProUGUI textoDisparo;

    public GameObject panelVersus;
    public float tiempoMostrarVersus = 2.5f;

    private bool juegoIniciado = false;

    void Start()
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);
        if (panelVersus != null) panelVersus.SetActive(false);
        textoCartas.text = "(Click para iniciar)";
    }

    void Update()
    {
        if (!juegoIniciado)
        {
            if (Input.GetMouseButtonDown(0))
            {
                juegoIniciado = true;
                StartCoroutine(SecuenciaVersus());
            }
        }
    }

    private IEnumerator SecuenciaVersus()
    {
        textoCartas.text = "";

        if (panelVersus != null) panelVersus.SetActive(true);

        yield return new WaitForSeconds(tiempoMostrarVersus);

        if (panelVersus != null) panelVersus.SetActive(false);

        IniciarFaseRecarga();
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
