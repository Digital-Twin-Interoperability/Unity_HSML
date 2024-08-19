using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateController : MonoBehaviour
{

    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("w"))
        {
            animator.SetBool("isWalking", true);
        }
        if (!Input.GetKey("w"))
        {
            animator.SetBool("isWalking", false);
        }
        if (Input.GetKey("h"))
        {
            animator.SetBool("isWaving", true);
        }
        if (!Input.GetKey("h"))
        {
            animator.SetBool("isWaving", false);
        }
        if (Input.GetKey("t"))
        {
            animator.SetBool("isPointing", true);
        }
        if (!Input.GetKey("t"))
        {
            animator.SetBool("isPointing", false);
        }
        if (Input.GetKey("f"))
        {
            animator.SetBool("isCrouching", true);
        }
        if (!Input.GetKey("f"))
        {
            animator.SetBool("isCrouching", false);
        }
        if (Input.GetKey("g"))
        {
            animator.SetBool("isNewWave", true);
        }
        if (!Input.GetKey("g"))
        {
            animator.SetBool("isNewWave", false);
        }
        if (Input.GetKey("c"))
        {
            animator.SetBool("isChecking", true);
        }
        if (!Input.GetKey("c"))
        {
            animator.SetBool("isChecking", false);
        }
    }
}
