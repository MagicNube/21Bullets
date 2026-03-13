using UnityEngine;

public class FasesManager : MonoBehaviour
{
    public GameObject contenedorCartas;
    public GameObject contenedorDisparo;

    public CartasManager cartasManager;
    public DisparoManager disparoManager;

    void Start()
    {
        IniciarFaseCartas();
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