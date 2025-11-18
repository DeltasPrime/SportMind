using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private float projectileSpeed = 60f;
    [SerializeField] private float projectileLifetime = 5f;

    public void Fire()
    {
        if (projectilePrefab == null || muzzlePoint == null)
            return;

        GameObject projectile = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = muzzlePoint.forward * projectileSpeed;
        }

        Destroy(projectile, projectileLifetime);
    }
}
