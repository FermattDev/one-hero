using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : AvatarController
{
    [SerializeField] private PlayerInput playerInput;

    private float timeStart;

    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionRecorder = new List<KeyValuePair<float, KeyValuePair<string, object>>>();
    private Task fireTask;

    public Action OnPlayerDead;

    private void Start()
    {
        timeStart = Time.time;
    }

    void OnEnable()
    {
        playerInput.actions["Move"].performed += AvatarMove;
        playerInput.actions["Fire"].performed += AvatarFire;
        playerInput.actions["Dash"].performed += AvatarDash;
    }

    void OnDisable()
    {
        playerInput.actions["Move"].performed -= AvatarMove;
        playerInput.actions["Fire"].performed -= AvatarFire;
        playerInput.actions["Dash"].performed -= AvatarDash;
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

        OnPlayerDead?.Invoke();

        base.AvatarDead();
    }

    private void AddActionToRecorder(string key, object value)
    {
        var actionValue = new KeyValuePair<string, object>(key, value);
        var action = new KeyValuePair<float, KeyValuePair<string, object>>(Time.time - timeStart, actionValue);
        actionRecorder.Add(action);
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

    public List<KeyValuePair<float, KeyValuePair<string, object>>> GetActionRecorder()
    {
        return actionRecorder;
    }
}
