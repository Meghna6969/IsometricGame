using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f;
    public float stoppingDistance = 1.5f;

    [Header("Randomization")]
    public float moveSpeedVariation = 0.3f;
    public float stoppingDistanceVariation = 0.5f;
    public float personalSpaceRadius = 1f;

    private Animator animator;
    private float actualMoveSpeed;
    private float actualStoppingDistance;
    private Vector3 randomOffset;
    private float animationOffset;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
        actualMoveSpeed = moveSpeed * Random.Range(1f - moveSpeedVariation, 1f + moveSpeedVariation);
        actualStoppingDistance = stoppingDistance * Random.Range(-stoppingDistanceVariation, stoppingDistanceVariation);

        randomOffset = new Vector3(Random.Range(-personalSpaceRadius, personalSpaceRadius), 0f, Random.Range(-personalSpaceRadius, personalSpaceRadius));

        animationOffset = Random.Range(0f, 1f);
        AnimatorStateInfo[] stateInfos = new AnimatorStateInfo[animator.layerCount];
        for (int i = 0; i < animator.layerCount; i++)
        {
            animator.Play(animator.GetCurrentAnimatorStateInfo(i).shortNameHash, i, animationOffset);
        }
        animator.speed = Random.Range(0.9f, 1.1f);
    }
    void Update()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position + randomOffset;
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance > actualStoppingDistance)
        {
            animator.SetBool("Attack", false);
            Vector3 direction = (player.position - transform.position).normalized;
            Vector3 separationForce = CalculateSeparation();
            direction = (direction + separationForce).normalized;
            transform.position += direction * actualMoveSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        else
        {
            animator.SetBool("Attack", true);
            Vector3 lookDirection = (player.position - transform.position).normalized;
            if(lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        if (Random.value < 0.01f)
        {
            randomOffset = new Vector3(Random.Range(-personalSpaceRadius, personalSpaceRadius), 0f, Random.Range(-personalSpaceRadius, personalSpaceRadius));
        }
    }
    private Vector3 CalculateSeparation()
    {
        Vector3 separationForce = Vector3.zero;
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, personalSpaceRadius * 2f);

        int neighborCount = 0;
        foreach (Collider col in nearbyEnemies)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                Vector3 awayFromNeighbor = transform.position - col.transform.position;
                float distance = awayFromNeighbor.magnitude;

                if (distance > 0f && distance < personalSpaceRadius * 2f)
                {
                    separationForce += awayFromNeighbor.normalized / distance;
                    neighborCount++;
                }
            }
        }
        if (neighborCount > 0)
        {
            separationForce /= neighborCount;
        }
        return separationForce;
    }
}
