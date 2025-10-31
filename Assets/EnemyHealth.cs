using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Damage Settings")]
    public float collisionDamage = 50f;
    public float damageResetTime = 1f;

    [Header("UI")]
    public Image heatlhBarFill;
    public Canvas healthBarCanvas;
    public Vector3 healthBarOffset = new Vector3(0, 2, 0);

    [Header("Death Settings")]
    public GameObject deathEffect;
    public float deathDelay = 0f;

    private Camera mainCamera;
    private float lastDamageTime = -999f;
    private GameObject lastDamageSource;
    void Start()
    {
        currentHealth = maxHealth;
        mainCamera = Camera.main;
        UpdateHealthBar();

    }
    void Update()
    {
        healthBarCanvas.transform.position = transform.position + healthBarOffset;
        healthBarCanvas.transform.rotation = Quaternion.LookRotation(healthBarCanvas.transform.position - mainCamera.transform.position);

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("EnemysEnemy"))
        {
            if (Time.time - lastDamageTime > damageResetTime || other.gameObject != lastDamageSource)
            {
                TakeDamage(collisionDamage);
                lastDamageTime = Time.time;
                lastDamageSource = other.gameObject;
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
        heatlhBarFill.fillAmount = currentHealth / maxHealth;
    }
    private void Die()
    {
        Destroy(gameObject, deathDelay);
    }
}
