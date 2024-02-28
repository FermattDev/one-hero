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
        gameObject.transform.LookAt(transform.position + direction.normalized, -Vector3.forward);
    }
}
