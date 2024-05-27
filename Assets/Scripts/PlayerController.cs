using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : AvatarController
{
    [SerializeField] private PlayerInput playerInput;

    private Task fireTask;

    void OnEnable()
    {
        playerInput.actions["Move"].performed += AvatarMove;
        playerInput.actions["Fire"].performed += AvatarFire;
        playerInput.actions["Dash"].performed += AvatarDash;

        OnEntityDead += AvatarDead;
    }

    void OnDisable()
    {
        playerInput.actions["Move"].performed -= AvatarMove;
        playerInput.actions["Fire"].performed -= AvatarFire;
        playerInput.actions["Dash"].performed -= AvatarDash;
        
        OnEntityDead -= AvatarDead;
    }

    protected void AvatarMove(InputAction.CallbackContext value)
    {
        var inputValue = value.ReadValue<Vector2>();

        base.AvatarMove(inputValue);

        AddActionToRecorder("Move", inputValue);
    }

    protected void AvatarFire(InputAction.CallbackContext value)
    {
        if(fireTask != null && !fireTask.IsCompleted)
        {
            return;
        }

        fireTask = base.AvatarFire(value);

        AddActionToRecorder("Fire", true);
    }

    protected void AvatarDash(InputAction.CallbackContext value)
    {
        base.AvatarDash(value);

        AddActionToRecorder("Dash", true);
    }

    protected override void AvatarDead()
    {
        AddActionToRecorder("Dead", true);

        base.AvatarDead();
    }

    protected override void Update()
    {
        base.Update();

        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);

        if (Input.GetKeyDown(KeyCode.R))
        {
            AvatarDead();
        }
    }
}
