using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonFix : MonoBehaviour, IPointerUpHandler
{
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        animator.Play("Normal", 0, 0);
    }

    void OnDisable()
    {
        animator.Play("Button", 0, 0);
        animator.Update(0);
    }
}