using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class RangerEnemyController : EnemyController
{
    protected override async Task AvatarFire()
    {
        await Task.Delay((int)(fireDelay * 1000));

        if (avatarTracker == null)
        {
            return;
        }
        
        Vector3 direction = avatarTracker.transform.position - transform.position;

        var projectile = Instantiate(projectilePrefab);
        projectile.transform.position = transform.position;
        projectile.SetProjectileDirection(direction.normalized);

        await base.AvatarFire();
    }

    protected override void Update()
    {
        var isTrackingAvatar = avatarTracker != null;
        
        if (isTrackingAvatar)
        {
            Vector3 direction = avatarTracker.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);
            transform.rotation = rotation;
        }
        
        if (!actionStart)
        {
            if(playerController == null)
            {
                return;
            }
            if(!isTrackingAvatar && (playerController.transform.position - transform.position).magnitude < detectionRange)
            {
                AvatarSetTracker(playerController.AvatarId);
            }
            if(isTrackingAvatar && (playerController.transform.position - transform.position).magnitude > detectionRange)
            {
                AvatarSetTracker(-1);
            }

            if (isTrackingAvatar)
            {
                if(fireTask == null || fireTask.IsCompleted)
                {
                    fireTask = AvatarFire();
                }
            }
            return;
        }
        
        base.Update();
    }
}
