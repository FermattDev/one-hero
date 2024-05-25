using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class EnemyController : EntityController
{
    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private float detectionRange = 3;
    [SerializeField] protected float fireDelay = 1f;

    private PlayerController playerController;
    private AvatarController avatarTracker;
    private bool actionStart;
    private Task fireTask;
    private int index = 0;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private LevelManager levelManager;

    protected override void Start()
    {
        base.Start();
        levelManager = ManagerLocator.Get<LevelManager>();
        levelManager.OnLevelReset += ResetEnemy;
        playerController = levelManager.GetPlayerController();
        initialRotation = transform.rotation;
        initialPosition = transform.position;
    }

    public void ResetEnemy(PlayerController player)
    {
        ResetEntity();
        playerController = player;
        actionStart = true;
        index = 0;
        transform.rotation = initialRotation;
        transform.position = initialPosition;
    }

    protected virtual void AvatarSetTracker(object value)
    {
        int avatarId = (int)value;

        if (avatarId < 0)
        {
            avatarTracker = null;
            return;
        }
        
        avatarTracker = levelManager.GetAvatarById(avatarId);
        
        if(!actionStart)
        {
            AddActionToRecorder("Tracker", value);
        }
    }

    protected virtual async Task AvatarFire()
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
        projectile.SetProjectileCreator(transform);

        if(!actionStart)
        {
            AddActionToRecorder("Fire", true);
        }
    }

    protected virtual void AvatarDead()
    {
        Destroy(gameObject);

        if (!actionStart)
        {
            AddActionToRecorder("Dead", true);
        }
    }

    private void Update()
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
        if (ActionRecorder.Count > index)
        {
            if ((ActionRecorder[index].Key) < Time.time - TimeStart)
            {
                switch (ActionRecorder[index].Value.Key)
                {
                    case "Fire":
                        fireTask = AvatarFire();
                        break;
                    case "Dead":
                        AvatarDead();
                        break;
                    case "Tracker":
                        AvatarSetTracker(ActionRecorder[index].Value.Value);
                        break;
                }

                index++;
            }
        }
        else
        {
            actionStart = false;
        }
    }
}
