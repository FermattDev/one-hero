using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float projectileSpeed;

    private Transform projectileCreator;

    private void Update()
    {
        transform.position += transform.forward * projectileSpeed * Time.deltaTime;
    }

    public void SetProjectileCreator(Transform creator)
    {
        projectileCreator = creator;
    }

    public void SetProjectileDirection(Vector3 direction)
    {
        gameObject.transform.LookAt(transform.position + direction.normalized, -Vector3.forward);
    }
}
