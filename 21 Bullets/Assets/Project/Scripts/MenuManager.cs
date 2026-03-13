using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject panelOpciones;
    public string nombreEscenaJuego;

    void Start()
    {
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }
    }

    public void BotonJugar()
    {
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void AbrirOpciones()
    {
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(true);
        }
    }

    public void CerrarOpciones()
    {
        if (panelOpciones != null)
        {
            panelOpciones.SetActive(false);
        }
    }

    public void BotonSalir()
    {
        Application.Quit();
    }
}
