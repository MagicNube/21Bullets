using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BotonUIArreglo : MonoBehaviour, IPointerUpHandler, IPointerExitHandler, ISelectHandler
{
    private Vector3 escalaOriginal = Vector3.one;
    private Button miBoton;

    void Awake()
    {
        miBoton = GetComponent<Button>();
    }

    void OnDisable()
    {
        transform.localScale = escalaOriginal;
    }

    public void OnSelect(BaseEventData eventData)
    {
        StartCoroutine(LimpiarEstado());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartCoroutine(LimpiarEstado());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(LimpiarEstado());
    }

    private System.Collections.IEnumerator LimpiarEstado()
    {
        yield return null; 
        
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (miBoton != null && miBoton.animator != null)
        {
            miBoton.animator.ResetTrigger("Pressed");
            miBoton.animator.ResetTrigger("Selected");
            miBoton.animator.ResetTrigger("Highlighted");
            miBoton.animator.SetTrigger("Normal");
        }
    }
}