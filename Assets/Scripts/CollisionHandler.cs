using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    public Action<Collider2D> OnTriggerEnter;
    public Action<Collider2D> OnTriggerExit;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggerEnter?.Invoke(other);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        OnTriggerExit?.Invoke(other);
    }
}
