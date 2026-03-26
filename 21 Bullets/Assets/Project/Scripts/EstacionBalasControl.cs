using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EstacionBalasControl : MonoBehaviour
{
    private Animator animator;
    private bool estaDesplegado = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Desplegar()
    {
        estaDesplegado = true;
        animator.Play("MarcoBalasDesplegar");
    }

    public void Ocultar()
    {
        if (estaDesplegado)
        {
            estaDesplegado = false;
            animator.Play("MarcoBalasOcultar");
        }
    }
}