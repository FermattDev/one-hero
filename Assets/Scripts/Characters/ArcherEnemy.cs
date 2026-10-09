using System.Threading.Tasks;
using UnityEngine;

public class ArcherEnemy : EnemyController
{
    [SerializeField] private ProjectileController projectilePrefab;

    protected override async Task AvatarFire(object value)
    {
        await Task.Delay((int)(fireCharge * 1000));

        if (health <= 0 || (playerController.transform.position - transform.position).magnitude > detectionRange)
        {
            return;
        }

        Transform target = (Transform)value;
        var direction = (target.position - transform.position).normalized;

        var projectile = Instantiate(projectilePrefab);
        projectile.transform.position = transform.position;
        projectile.SetProjectileDirection(direction.normalized);
    }
}
