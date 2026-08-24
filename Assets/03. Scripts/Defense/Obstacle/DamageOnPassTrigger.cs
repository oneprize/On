using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageOnPassTrigger : MonoBehaviour
{
    [SerializeField] private int damage = 20;

    private readonly HashSet<Collider> hitThisPass = new HashSet<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponent<MonsterHealth>();
        if (health == null) return;
        if (!hitThisPass.Add(other)) return;

        health.TakeDamage(damage);
    }

    private void OnTriggerExit(Collider other)
    {
        hitThisPass.Remove(other);
    }
}
