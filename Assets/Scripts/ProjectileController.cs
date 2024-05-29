using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private CollisionHandler collisionHandler;
    [SerializeField] private float projectileSpeed;
    
    private Vector3 proyectileDirection;

    private void Start()
    {
        collisionHandler.OnTriggerEnter += DestroyProjectile;
    }

    private void Update()
    {
        transform.position += proyectileDirection * projectileSpeed * Time.deltaTime;
    }

    public void SetProjectileDirection(Vector3 direction)
    {
        proyectileDirection = direction;
    }

    private void DestroyProjectile(Collider2D other)
    {
        Destroy(gameObject);
    }
}
