using UnityEngine;

public class PlayerProjectileController : ProjectileController
{
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        var enemy = other.GetComponentInParent<EnemyController>();
        if (enemy == null || !enemy.IsAlive)
        {
            return;
        }

        enemy.TakeDamage(1);
        Destroy(gameObject);
    }
}
