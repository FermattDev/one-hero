using System.Threading.Tasks;
using UnityEngine;

public class SpinnerEnemy : EnemyController
{
    private const int DirectionCount = 8;

    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private float spinSpeed = 90f;

    protected override void Behave()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        if ((playerController.transform.position - transform.position).magnitude > detectionRange)
        {
            return;
        }

        if (fireTask is { IsCompleted: false })
        {
            return;
        }

        fireTask = AvatarFire(playerController.transform);
    }

    protected override async Task AvatarFire(object value)
    {
        await Task.Delay((int)(fireCharge * 1000));

        if (health <= 0 || (playerController.transform.position - transform.position).magnitude > detectionRange)
        {
            return;
        }

        for (int i = 0; i < DirectionCount; i++)
        {
            Vector3 direction = Quaternion.Euler(0, 0, i * (360f / DirectionCount)) * transform.up;

            var projectile = Instantiate(projectilePrefab);
            projectile.transform.position = transform.position;
            projectile.SetProjectileDirection(direction);
        }
    }
}
