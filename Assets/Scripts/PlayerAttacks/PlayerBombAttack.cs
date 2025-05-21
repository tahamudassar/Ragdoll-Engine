using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEngine;

public class PlayerBombAttack : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float attackRange;
    [SerializeField] private float bombTime=1f;
    [SerializeField] private LayerMask excludeLayers;
    [SerializeField] private GameObject bombParticles;

    private float bombTimer;
    private bool explode = false;
    public void Start()
    {
        bombTimer = bombTime;
    }

    private void FixedUpdate()
    {
        if(bombTimer > 0 && !explode)
        {
            bombTimer -= Time.fixedDeltaTime;
        }
        else
        {
            Explode();
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        explode = true;
    }


    private void Explode()
    {
        //Check all layers except exclude layers
        LayerMask mask = ~excludeLayers;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange,mask);
        Instantiate(bombParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}


