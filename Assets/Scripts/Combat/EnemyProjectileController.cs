using UnityEngine;

public class EnemyProjectileController : ProjectileController
{
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        player.TakeDamage(1);
        Destroy(gameObject);
    }
}
