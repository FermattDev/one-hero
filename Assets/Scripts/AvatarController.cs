using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class AvatarController : MonoBehaviour
{
    [SerializeField] protected float playerSpeed = 1;
    [SerializeField] protected float dashMaxSpeed = 2;
    [SerializeField] protected float dashMinSpeed = 0.3f;
    [SerializeField] protected float dashTotalTime = 0.5f;
    [SerializeField] protected float fireDelay = 0.5f;

    [SerializeField] private ProjectileController projectilePrefab;

    private Vector3 currentMovement = Vector3.zero;
    private Vector3 currentDashMovement = Vector3.zero;

    private float dashSpeed = 0;

    protected virtual void AvatarMove(object value)
    {
        Vector2 vec = (Vector2) value;
        vec = vec * playerSpeed;
        var movement = new Vector3(vec.x, vec.y, 0);
        currentMovement = movement;
    }

    protected virtual async Task AvatarFire(object value)
    {
        var projectile = Instantiate(projectilePrefab);
        projectile.transform.position = transform.position;
        projectile.SetProjectileDirection(currentMovement);
        projectile.SetProjectileCreator(transform);

        await Task.Delay((int)(fireDelay * 1000));
    }

    protected virtual void AvatarDash(object value)
    {
        currentDashMovement = currentMovement;
        TriggerDash();
    }

    protected virtual void AvatarDead()
    {
        Destroy(gameObject);
    }

    protected virtual void Update()
    {
        if (dashSpeed < dashMinSpeed)
        {
            transform.position += currentMovement * Time.deltaTime;
        }
    }

    protected async void TriggerDash()
    {
        var dashTime = 0f;
        dashSpeed = dashMaxSpeed;

        while (dashSpeed > dashMinSpeed)
        {
            transform.position += currentDashMovement * dashSpeed * Time.deltaTime;
            dashSpeed = Mathf.Lerp(dashMaxSpeed, dashMinSpeed, dashTime / dashTotalTime);
            dashTime += Time.deltaTime;
            await Task.Yield();
        }

        dashSpeed = 0f;
    }
}
