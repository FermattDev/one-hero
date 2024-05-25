using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class EntityController : MonoBehaviour
{
    private float timeStart;
    private List<KeyValuePair<float, KeyValuePair<string, object>>> actionRecorder = new List<KeyValuePair<float, KeyValuePair<string, object>>>();

    protected float TimeStart => timeStart;
    protected List<KeyValuePair<float, KeyValuePair<string, object>>> ActionRecorder => actionRecorder;

    protected virtual void Start()
    {
        ResetEntity();
    }
    
    protected void AddActionToRecorder(string key, object value)
    {
        var actionValue = new KeyValuePair<string, object>(key, value);
        var action = new KeyValuePair<float, KeyValuePair<string, object>>(Time.time - timeStart, actionValue);
        actionRecorder.Add(action);
    }
    
    public List<KeyValuePair<float, KeyValuePair<string, object>>> GetActionRecorder()
    {
        return actionRecorder;
    }

    protected void ResetEntity()
    {
        timeStart = Time.time;
    }
}
