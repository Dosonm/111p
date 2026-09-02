// Copyright (c) 2025 CriticalVFX
// This script is part of the Stylized Projectile FX Kit Mini Vol1 package.
// For licensing information, please refer to the included LICENSE file.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CVFX_SPFXKMINIV2
{
    public class ProjectileShooter_DamageOverTime_SPFXKV2 : MonoBehaviour
    {
        [System.Serializable]
        public class ProjectileSet
        {
            public GameObject[] prefabs;
        }

        public ProjectileSet[] projectilePrefabs;

        [System.Serializable]
        public class CollisionEffectSet
        {
            public GameObject[] effects;
        }

        public CollisionEffectSet[] collisionEffectPrefabs;
        [System.Serializable]
        public class EndEffectSet
        {
            public GameObject[] effects;
        }

        public EndEffectSet[] endEffectSet;
        public Transform firePoint;
        public Camera mainCamera;
        public float projectileSpeed = 20f;
        public float projectileLifetime = 5f;
        public Slider speedSlider;

        private float minProjectileSpeed = 0f;
        private float maxProjectileSpeed = 50;
        private float scrollSensitivity = 5f;
        private int currentProjectileSet = 0;
        private int currentProjectileIndex = 0;

        void Start()
        {
            if (speedSlider != null)
            {
                speedSlider.minValue = minProjectileSpeed;
                speedSlider.maxValue = maxProjectileSpeed;
                speedSlider.value = projectileSpeed;
            }
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q)) currentProjectileSet = 0;
            if (Input.GetKeyDown(KeyCode.W)) currentProjectileSet = 1;
            if (Input.GetKeyDown(KeyCode.E)) currentProjectileSet = 2;

            for (int i = 0; i < 8; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    currentProjectileIndex = i;
                }
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                projectileSpeed += scroll * scrollSensitivity;
                projectileSpeed = Mathf.Clamp(projectileSpeed, minProjectileSpeed, maxProjectileSpeed);
            }

            if (speedSlider != null)
            {
                speedSlider.value = projectileSpeed;
            }

            if (Input.GetMouseButtonDown(0))
            {
                ShootProjectile();
            }
        }

        void ShootProjectile()
        {
            if (mainCamera == null) mainCamera = Camera.main;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.origin + ray.direction * 100f;
            }

            Vector3 flatTarget = new Vector3(targetPoint.x, firePoint.position.y, targetPoint.z);
            Vector3 direction = (flatTarget - firePoint.position).normalized;

            GameObject[] currentArray = GetCurrentProjectileArray();
            if (currentArray == null || currentProjectileIndex >= currentArray.Length) return;

            GameObject projectilePrefab = currentArray[currentProjectileIndex];
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
            projectile.transform.forward = direction;
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }

            if (projectile.GetComponent<ProjectileDOT>() == null)
                projectile.AddComponent<ProjectileDOT>();
            var dot = projectile.GetComponent<ProjectileDOT>();
            dot.Initialize(projectileLifetime);

            GameObject[] currentCollisionArray = collisionEffectPrefabs[0].effects;
            dot.explosionEffectPrefab = currentCollisionArray[currentProjectileIndex];

            GameObject[] currentEndEffectArray = endEffectSet[currentProjectileSet].effects;
            dot.endEffectPrefab = currentEndEffectArray[currentProjectileIndex];

            dot.projectileTransform = projectile.transform;
        }

        GameObject[] GetCurrentProjectileArray()
        {
            if (currentProjectileSet < projectilePrefabs.Length)
                return projectilePrefabs[currentProjectileSet].prefabs;
            return null;
        }
    }

    public class ProjectileDOT : MonoBehaviour
    {
        public GameObject explosionEffectPrefab;
        public GameObject endEffectPrefab;
        public Transform projectileTransform;

        private float lifeTime = 5f;
        public float damageInterval = 1f;
        private float timeSinceLastDamage = 0f;
        private List<Collider> targetsInRange = new List<Collider>();

        public float collisionEffectInterval = 0.25f;
        private float collisionEffectTimer = 0f;

        public void Initialize(float duration)
        {
            lifeTime = duration;
            StartCoroutine(DestroyAfterLifetime());
        }

        private IEnumerator DestroyAfterLifetime()
        {
            yield return new WaitForSeconds(lifeTime);
            if (endEffectPrefab != null)
            {
                Instantiate(endEffectPrefab, transform.position, transform.rotation);
            }
            Destroy(gameObject);
        }

        private void Update()
        {
            timeSinceLastDamage += Time.deltaTime;
            collisionEffectTimer += Time.deltaTime;

            if (timeSinceLastDamage >= damageInterval)
            {
                foreach (var target in targetsInRange)
                {
                    if (target != null)
                    {
                    }
                }
                timeSinceLastDamage = 0f;
            }

            if (collisionEffectTimer >= collisionEffectInterval && targetsInRange.Count > 0)
            {
                foreach (var target in targetsInRange)
                {
                    if (target != null && explosionEffectPrefab != null)
                    {
                        Instantiate(explosionEffectPrefab, target.transform.position, Quaternion.LookRotation(projectileTransform.forward));
                    }
                }
                collisionEffectTimer = 0f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enermy") && !targetsInRange.Contains(other))
            {
                targetsInRange.Add(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Enermy" && targetsInRange.Contains(other))
            {
                targetsInRange.Remove(other);
            }
        }
    }
}