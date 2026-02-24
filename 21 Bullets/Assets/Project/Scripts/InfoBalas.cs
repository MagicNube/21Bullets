using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InfoBalas : MonoBehaviour
{
    public Sprite spriteBalaReal;
    public Sprite spriteBalaFogueo;

    public List<Image> slotsBalas;
    public TextMeshProUGUI textoReales;
    public TextMeshProUGUI textoFogueo;

    public AudioClip sonidoDisparoReal;
    public AudioClip sonidoDisparoFogueo;

    private AudioSource audioSource;
    private List<bool> cargadorLogico = new List<bool>();
    private int indiceActual = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("Falta AudioSource en HUD MANAGERS. Por favor añade uno.");
        }

        PrepararCargadorInicial();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DispararSiguienteBala();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PrepararCargadorInicial();
        }
    }

    public void PrepararCargadorInicial()
    {
        indiceActual = 0;
        cargadorLogico.Clear();

        // 1. Decidimos la cantidad de balas (Buckshot Style)
        int numReales = Random.Range(2, 4);
        int numFogueo = 6 - numReales;

        for (int i = 0; i < numReales; i++) cargadorLogico.Add(true);
        for (int i = 0; i < numFogueo; i++) cargadorLogico.Add(false);

        // 2. Barajamos el orden oculto
        for (int i = 0; i < cargadorLogico.Count; i++)
        {
            bool temp = cargadorLogico[i];
            int randomIndex = Random.Range(i, cargadorLogico.Count);
            cargadorLogico[i] = cargadorLogico[randomIndex];
            cargadorLogico[randomIndex] = temp;
        }

        ActualizarTextosUI(numReales, numFogueo);
    }

    void DispararSiguienteBala()
    {
        if (indiceActual >= cargadorLogico.Count)
        {
            Debug.Log("Cargador vacío. Pulsa R para recargar.");
            return;
        }

        bool esReal = cargadorLogico[indiceActual];
        Image slotUI = slotsBalas[indiceActual];

        if (slotUI != null)
        {
            if (esReal)
            {
                if (spriteBalaReal != null) slotUI.sprite = spriteBalaReal;
                if (audioSource != null && sonidoDisparoReal != null)
                {
                    audioSource.PlayOneShot(sonidoDisparoReal);
                }
            }
            else
            {
                if (spriteBalaFogueo != null) slotUI.sprite = spriteBalaFogueo;
                if (audioSource != null && sonidoDisparoFogueo != null)
                {
                    audioSource.PlayOneShot(sonidoDisparoFogueo);
                }
            }

            slotUI.color = Color.white; // Opacidad total al revelar
        }

        indiceActual++;
    }

    void ActualizarTextosUI(int reales, int fogueo)
    {
        // Corregido el posible error CS0131 usando comprobaciones estándar
        if (textoReales != null)
        {
            textoReales.text = reales + "x Reales";
        }

        if (textoFogueo != null)
        {
            textoFogueo.text = fogueo + "x Fogueo";
        }
    }
}
