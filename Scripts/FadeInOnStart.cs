using UnityEngine;

public class FadeInOnStart : MonoBehaviour
{
    [SerializeField] Animator transitionAnim;

    private void Start()
    {
        transitionAnim.SetTrigger("Start");
    }
}
