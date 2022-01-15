using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unattachedAnimator : MonoBehaviour
{

    [SerializeField]
    private Animator snipinator;

    bool charging;
    float charge;
    float cooldown;

    void Update()
    {

        if (!snipinator)
            return;

        Shooting();
        Moob();
    }

    private void Moob()
    {
        //man it's been so long since I've written a moob function

        float hori = Input.GetAxis("Horizontal");
        float veri = Input.GetAxis("Vertical");

        snipinator.SetBool("isMoving", Mathf.Abs(hori + veri) > 0.05f);
        snipinator.SetFloat("forwardSpeed", veri);
        snipinator.SetFloat("rightSpeed", hori);
    }

    private void Shooting()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (charging)
            {
                charging = false;
                charge = 0;
                cooldown = 5;

                snipinator.Play("FireGunStrong");
            }
            else
            {
                snipinator.Play("FireGun");
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            snipinator.SetBool("scoped", true);
            snipinator.Play("SteadyAimCharge");
        }

        if (Input.GetMouseButton(1))
        {
            if (cooldown < 0)
            {
                charging = true;
                charge += Time.deltaTime / 3;

                snipinator.SetFloat("SecondaryCharge", charge);
            }
            cooldown -= Time.deltaTime;
        }

        if (Input.GetMouseButtonUp(1))
        {
            snipinator.SetBool("scoped", false);
        }
    }
}
