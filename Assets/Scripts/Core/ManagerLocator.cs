using System;
using System.Collections.Generic;
using UnityEngine;

public class ManagerLocator
{
    private static ManagerLocator instance;
    private Dictionary<Type, Manager> managers = new Dictionary<Type, Manager>();

    public T GetManager<T>()
    {
        foreach (var manager in managers)
        {
            if (manager.Key == typeof(T))
            {
                return (T)manager.Value;
            }
        }
        Debug.LogError($"Manager of type {typeof(T)} does not exist");
        return default;
    }

    public bool SetManager<T>(Manager newManager)
    {
        if (managers.TryGetValue(typeof(T), out var existing))
        {
            // The locator outlives scene loads, so a manager from a previous scene may be a destroyed leftover.
            if (existing is UnityEngine.Object unityObject && unityObject == null)
            {
                managers.Remove(typeof(T));
            }
            else
            {
                Debug.LogError($"Manager of type {typeof(T)} already exists in the locator");
                return false;
            }
        }
        managers.Add(typeof(T), newManager);
        return true;
    }

    public void RemoveManager<T>(Manager manager)
    {
        if (managers.TryGetValue(typeof(T), out var existing) && ReferenceEquals(existing, manager))
        {
            managers.Remove(typeof(T));
        }
    }

    public static bool Set<T>(Manager newManager)
    {
        return GetInstance().SetManager<T>(newManager);
    }

    public static void Remove<T>(Manager manager)
    {
        GetInstance().RemoveManager<T>(manager);
    }

    public static T Get<T>()
    {
        return GetInstance().GetManager<T>();
    }

    private static ManagerLocator GetInstance()
    {
        if(instance == null)
        {
            instance = new ManagerLocator();
        }
        return instance;
    }
}
