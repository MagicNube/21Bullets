using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ModoDificultad { Normal, Dificil, Personalizado }

public class DificultadManager : MonoBehaviour
{
    [Header("UI Modo Personalizado")]
    public Slider sliderCuracion;
    public TMP_Dropdown dropdownObjetos;
    public Toggle toggleBalas;
    public TMP_Dropdown dropdownLimites;

    public static ModoDificultad DificultadElegida = ModoDificultad.Normal;
    
    public static bool CP_CuraTotal = true;
    public static int CP_VidasIniciales = 3;
    public static int CP_DiferenciaObjetos = 0; 
    public static bool CP_VerBalas = true;
    public static bool CP_SinLimitesObjetos = true;

    public void ElegirNormal()
    {
        DificultadElegida = ModoDificultad.Normal;
        LanzarJuego();
    }

    public void ElegirDificil()
    {
        DificultadElegida = ModoDificultad.Dificil;
        LanzarJuego();
    }

    public void ElegirPersonalizado()
    {
        DificultadElegida = ModoDificultad.Personalizado;

        if (sliderCuracion != null)
        {
            CP_VidasIniciales = (int)sliderCuracion.value;
            CP_CuraTotal = (CP_VidasIniciales >= 3);
        }

        if (dropdownObjetos != null)
        {
            CP_DiferenciaObjetos = -dropdownObjetos.value;
        }

        if (toggleBalas != null)
        {
            CP_VerBalas = toggleBalas.isOn;
        }

        if (dropdownLimites != null)
        {
            CP_SinLimitesObjetos = (dropdownLimites.value == 0);
        }

        LanzarJuego();
    }

    private void LanzarJuego()
    {
        MenuManager menu = FindObjectOfType<MenuManager>();
        if (menu != null)
        {
            menu.ConfirmarJuego();
        }
    }
}