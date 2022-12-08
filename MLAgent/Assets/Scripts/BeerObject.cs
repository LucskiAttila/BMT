using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeerObject : MonoBehaviour
{
    Animator beerAnimator;

    public int attackNumber;

    void Start()
    {
        beerAnimator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        beerAnimator.Play("Attack" + attackNumber);
    }
}
