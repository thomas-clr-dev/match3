using UnityEngine;
using System.Collections.Generic;
using System;

public static class ServiceLocator
{

    private static readonly Dictionary<Type, IGameService> _services = new Dictionary<Type, IGameService>();

    public static void Register<T>(T service) where T : IGameService
    {
        Type type = typeof(T);
        if (!_services.ContainsKey(type))
        {
            _services.Add(type, service);
        }
        else
        {
            _services[type] = service; // Update the old value if we re the scene
        }
    }

    public static T Get<T>() where T : IGameService
    {
        if (_services.TryGetValue(typeof(T), out var service))
            return (T)service;

        Debug.LogError($"Service {typeof(T)} introuvable ! As-tu mis le script dans la scène ?");
        return default;
    }

}
