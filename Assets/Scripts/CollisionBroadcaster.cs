using System;
using UnityEngine;

/// <summary>
/// Broadcasts collision events through action events.
/// </summary>
public class CollisionBroadcaster : MonoBehaviour
{

    // ___ EVENTS ___
    public Action<Collision> OnCollisionEnterEvent;
    public Action<Collision> OnCollisionExitEvent;
    public Action<Collision> OnCollisionStayEvent;

    // ___ UNITY METHODS ___
    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnterEvent?.Invoke(collision);
    }
    private void OnCollisionExit(Collision collision)
    {
        OnCollisionExitEvent?.Invoke(collision);
    }
    private void OnCollisionStay(Collision collision)
    {
        OnCollisionStayEvent?.Invoke(collision);
    }
}
