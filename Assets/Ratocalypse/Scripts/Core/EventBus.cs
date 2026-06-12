// ============================================================
//  EventBus.cs
//  Ratpocalypse — Core/EventBus.cs
//
//  Globalny system zdarzeń oparty na typach.
//  Systemy komunikują się przez zdarzenia zamiast twardych
//  referencji — luźne powiązanie, łatwe testowanie.
//
//  Użycie:
//    EventBus.Subscribe<OnPlayerDamaged>(OnDamaged);
//    EventBus.Publish(new OnPlayerDamaged { amount = 10f });
//    EventBus.Unsubscribe<OnPlayerDamaged>(OnDamaged);
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    // Słownik: typ zdarzenia → lista callbacków (jako object żeby uniknąć reflection)
    static readonly Dictionary<Type, List<object>> subscribers = new();

    // --------------------------------------------------------
    // Subscribe — rejestracja nasłuchiwacza
    // --------------------------------------------------------
    public static void Subscribe<T>(Action<T> callback) where T : struct
    {
        var type = typeof(T);
        if (!subscribers.ContainsKey(type))
            subscribers[type] = new List<object>();

        subscribers[type].Add(callback);
    }

    // --------------------------------------------------------
    // Unsubscribe — wyrejestrowanie (ZAWSZE wywoływać w OnDestroy)
    // --------------------------------------------------------
    public static void Unsubscribe<T>(Action<T> callback) where T : struct
    {
        var type = typeof(T);
        if (subscribers.TryGetValue(type, out var list))
            list.Remove(callback);
    }

    // --------------------------------------------------------
    // Publish — wysłanie zdarzenia do wszystkich nasłuchiwaczy
    // --------------------------------------------------------
    public static void Publish<T>(T eventData) where T : struct
    {
        var type = typeof(T);
        if (!subscribers.TryGetValue(type, out var list))
            return;

        // Iterujemy po kopii żeby uniknąć problemów gdy callback odpina się podczas iteracji
        var snapshot = new List<object>(list);
        foreach (var sub in snapshot)
        {
            try
            {
                ((Action<T>)sub)?.Invoke(eventData);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EventBus] Błąd w callbacku dla {type.Name}: {e}");
            }
        }
    }

    // --------------------------------------------------------
    // Clear — czyszczenie wszystkich subskrypcji (np. przy zmianie sceny)
    // --------------------------------------------------------
    public static void Clear()
    {
        subscribers.Clear();
    }

    // --------------------------------------------------------
    // Debug helper — ile nasłuchiwaczy na dany typ
    // --------------------------------------------------------
    public static int SubscriberCount<T>() where T : struct
    {
        return subscribers.TryGetValue(typeof(T), out var list) ? list.Count : 0;
    }
}