using UnityEngine;
using TMPro;

public class FasesManager : MonoBehaviour
{
    public GameObject contenedorCartas;
    public GameObject contenedorDisparo;

    public CartasManager cartasManager;
    public DisparoManager disparoManager;

    public TextMeshProUGUI textoPantalla;

    private bool juegoIniciado = false;

    void Start()
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(false);
        textoPantalla.text = "Barajando las cartas... \n (Presiona ESPACIO para iniciar el juego)";
    }

    void Update()
    {
        if (!juegoIniciado)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                juegoIniciado = true;
                IniciarFaseCartas();
            }
        }
    }

    public void IniciarFaseCartas()
    {
        contenedorDisparo.SetActive(false);
        contenedorCartas.SetActive(true);
        cartasManager.ResetSequence();
    }

    public void IniciarFaseDisparo(bool ganaJugador)
    {
        contenedorCartas.SetActive(false);
        contenedorDisparo.SetActive(true);
        disparoManager.IniciarFase(ganaJugador);
    }
}