using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class TankEnemy : MonoBehaviour ,BaseEnemy
{
    private NavMeshAgent navmeshAgent;
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;

    //Idle state
    [SerializeField] private float playerDetectionDistance = 30;

    //Chasing State
    [SerializeField] private float chaseDetectionDistance = 70;
    [SerializeField] private float backupDistance;
    [SerializeField] private float backupSpeed = 2f; // Speed at which the tank moves backwards when the player is too close


    //Shooting State
    [SerializeField] private float shootDistance = 40;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackCooldown = 1f;
    private float attackCooldownTimer = 0f;
    [SerializeField] private float attackAngleThreshold = 0.7f;
    [SerializeField] private Transform projectilePrefab;

    //Death state
    [SerializeField] private Transform deathEffect;

    public enum State
    {
        Idle,
        Chasing,
        Cooldown
    }

    private State state;
    public Action<State> OnStateChange;
    private Action OnDeath;
    private Action OnHit;
    Action BaseEnemy.OnDeath
    {
        get => OnDeath;
        set => OnDeath = value;
    }

    Action IHittable.OnHit
    {
        get => OnHit;
        set => OnHit = value;
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        navmeshAgent = GetComponent<NavMeshAgent>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        state = State.Idle;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TickCooldowns();
        switch (state)
        {
            case State.Idle:
                Idle();
                break;
            case State.Chasing:
                Chasing();
                break;
            case State.Cooldown:
                Cooldown();
                break;
            default:
                break;
        }
    }
    private void TickCooldowns()
    {
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.fixedDeltaTime;
        }
    }
    private void Idle()
    {
        navmeshAgent.destination = transform.position; // Stop moving during idle state
        navmeshAgent.updateRotation = false; // Stop the NavMeshAgent from rotating during idle state
        Vector3 playerPos = Player.CharacterInstance.playerBehaviourTree.modelTransform.position;
        if (Vector3.SqrMagnitude(transform.position - playerPos) <= playerDetectionDistance * playerDetectionDistance)
        {
            state = State.Chasing;
            OnStateChange?.Invoke(state);
        }
    }

    private void Chasing()
    {
        Vector3 playerPos = Player.CharacterInstance.playerBehaviourTree.modelTransform.position;
        //Set navmeshDestination to player
        navmeshAgent.destination = playerPos;
        navmeshAgent.updateRotation = true; // Allow NavMeshAgent to rotate towards the player

        //Check if player is out of range
        float distance = Vector3.SqrMagnitude(transform.position - playerPos);
        if (distance >= chaseDetectionDistance * chaseDetectionDistance)
        {
            navmeshAgent.destination = transform.position;
            state = State.Idle;
            OnStateChange?.Invoke(state);
        }
        //If player is too close then move backwards
        else if (distance <= backupDistance * backupDistance)
        {

            transform.position = transform.position - transform.forward * Time.fixedDeltaTime * backupSpeed; // Move backwards at a speed of 2 units per second
            navmeshAgent.destination = transform.position; // Update the NavMeshAgent's destination to the new position
            navmeshAgent.updateRotation = false; // Stop the NavMeshAgent from rotating while backing up
        }
        //If player in shooting range then try to shoot
        else if (distance <= shootDistance * shootDistance)
        {
            Shoot();
        }


    }

    private void Shoot()
    {
        if (attackCooldownTimer <= 0f)
        {
            //Get playerpos and check if player is in a certain cone in front of tank using angles
            //If player is in cone launch attack directly from the tank attackPoint
            Vector3 playerPos = Player.CharacterInstance.playerBehaviourTree.modelTransform.position;
            Vector3 playerDir = playerPos - transform.position;
            playerDir.Normalize();
            //Get angle between playerDir and transform.forward
            float dot = Vector3.Dot(playerDir, transform.forward);

            if (dot >= attackAngleThreshold)
            {
                Instantiate(projectilePrefab, attackPoint.position, attackPoint.rotation);

                attackCooldownTimer = attackCooldown;
                navmeshAgent.destination = transform.position;
                state = State.Cooldown;
                OnStateChange?.Invoke(state);
            }

        }
    }

    private void Cooldown()
    {
        navmeshAgent.updateRotation = false; // Stop the NavMeshAgent from rotating during cooldown
        if (attackCooldownTimer <= 0f)
        {
            state = State.Chasing;
            navmeshAgent.updateRotation = true; // Resume rotation when not in cooldown
            OnStateChange?.Invoke(state);
        }
    }
    public float GetHealthNormalized()
    {
        return (float)currentHealth / (float)maxHealth;
    }

    public void DoHit(int damage)
    {
        currentHealth -= damage;
        OnHit?.Invoke();
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }

    HittableType IHittable.GetType()
    {
        return HittableType.Enemy;
    }
}
