using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Damage Settings")]
    public float damageResetTime = 1f;
    [Header("UI")]
    public Image healthBarFill;
    public Canvas healthBarCanvas;
    public Vector3 healthBarOffset = new Vector3(0, 2, 0);

    [Header("Death Settings")]
    public GameObject deathEffect;
    public float deathDelay = 0f;

    private Camera mainCamera;
    private Dictionary<GameObject, float> lastDamageTimeByObject = new Dictionary<GameObject, float>();

    void Start()
    {
        currentHealth = maxHealth;
        mainCamera = Camera.main;
        UpdateHealthBar();
    }
    
    void Update()
    {
        if (healthBarCanvas != null && mainCamera != null)
        {
            healthBarCanvas.transform.position = transform.position + healthBarOffset;
            healthBarCanvas.transform.rotation = Quaternion.LookRotation(healthBarCanvas.transform.position - mainCamera.transform.position);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        PickupObject pickupObj = other.GetComponent<PickupObject>();
        if (pickupObj != null)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null && rb.linearVelocity.magnitude > pickupObj.damageVelocityThreshold)
            {
               
                if (!lastDamageTimeByObject.ContainsKey(other.gameObject) || 
                    Time.time - lastDamageTimeByObject[other.gameObject] > damageResetTime)
                {
                    TakeDamage(pickupObj.throwDamage);
                    lastDamageTimeByObject[other.gameObject] = Time.time;
                    
                    Debug.Log($"{gameObject.name} took {pickupObj.throwDamage} damage from {other.name}. Health: {currentHealth}/{maxHealth}");
                }
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }
    
    private void Die()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        if (healthBarCanvas != null)
        {
            Destroy(healthBarCanvas.gameObject);
        }
        
        Destroy(gameObject, deathDelay);
    }
    
    void OnDestroy()
    {
        if (healthBarCanvas != null)
        {
            Destroy(healthBarCanvas.gameObject);
        }
    }
}