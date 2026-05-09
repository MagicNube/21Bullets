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
        if (gameObject.activeInHierarchy)
        {
            animator.Play("MarcoBalasDesplegar");
        }
    }

    public void Ocultar()
    {
        if (estaDesplegado)
        {
            estaDesplegado = false;
            if (gameObject.activeInHierarchy)
            {
                animator.Play("MarcoBalasOcultar");
            }
        }
    }
}