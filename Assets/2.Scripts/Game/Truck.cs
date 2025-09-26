using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Truck : MonoBehaviour
{
    [Header("Tower Stats")]
    public float maxHealth = 1000f;
    public float currentHealth;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public Transform destination;
    public float stoppingDistance = 0.5f;

    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (destination == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, destination.position);

        if (distanceToTarget > stoppingDistance)
        {
            Vector3 moveDirection = (destination.position - transform.position).normalized;
            rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
        }
        else
        {
            rb.velocity = Vector3.zero; // 목표 지점에 도착하면 멈춤
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
