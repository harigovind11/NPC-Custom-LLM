using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class TalkAnimationSync : MonoBehaviour
{
    
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void StartTalking(float duration)
    {
        StopAllCoroutines(); // Stop previous coroutine if any
        StartCoroutine(TalkCoroutine(duration));
    }

    private IEnumerator TalkCoroutine(float duration)
    {
        _animator.SetBool("Talk", true);
        yield return new WaitForSeconds(duration);
        _animator.SetBool("Talk", false); // Back to Idle
    }
}