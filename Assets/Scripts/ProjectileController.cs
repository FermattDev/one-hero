using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float projectileSpeed;
    [SerializeField] private List<string> collisionTags;

    private Transform projectileCreator;
    private Vector3 proyectileDirection;

    private void Update()
    {
        transform.position += proyectileDirection * projectileSpeed * Time.deltaTime;

        if(Camera.main != null)
        {
            var v = Camera.main.WorldToViewportPoint(transform.position);
            if(v.x > 0 && v.x < 1 && v.y > 0 && v.y < 1 && v.z > 0)
            {
                return;
            }

            Destroy(gameObject);
        }
    }

    public void SetProjectileCreator(Transform creator)
    {
        projectileCreator = creator;
    }

    public void SetProjectileDirection(Vector3 direction)
    {
        proyectileDirection = direction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collisionTags.Contains(other.transform.tag))
        {
            var controller = other.transform.GetComponent<EntityController>();
            if (controller != null)
            {
                controller.KillEntity();
                Destroy(gameObject);
            }
        }
    }
}
