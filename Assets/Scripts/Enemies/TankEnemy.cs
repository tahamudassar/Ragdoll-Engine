using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class TankEnemy : MonoBehaviour
{
    private NavMeshAgent navmeshAgent;
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;

    //Idle state
    [SerializeField] private float playerDetectionDistance=30;

    //Chasing State
    [SerializeField] private float chaseDetectionDistance=70;



    //Shooting State
    [SerializeField] private float shootDistance = 40;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackCooldown = 1f;
    private float attackCooldownTimer = 0f;
    [SerializeField] private float attackAngleThreshold = 0.7f;
    [SerializeField] private Transform projectilePrefab;

    public enum State
    {
        Idle,
        Chasing,
        Cooldown
    }

    private State state;
    public Action<State> OnStateChange;
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
    void Update()
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
            attackCooldownTimer -= Time.deltaTime;
        }
    }
    private void Idle()
    {

        Vector3 playerPos = Player.Instance.CharacterInstance.playerBehaviourTree.modelTransform.position;
        if (Vector3.SqrMagnitude(transform.position - playerPos) <= playerDetectionDistance * playerDetectionDistance)
        {
            state = State.Chasing;
            OnStateChange?.Invoke(state);
        }
    }

    private void Chasing()
    {
        Vector3 playerPos = Player.Instance.CharacterInstance.playerBehaviourTree.modelTransform.position;
        //Set navmeshDestination to player
        navmeshAgent.destination = playerPos;

        //Check if player is out of range
        float distance = Vector3.SqrMagnitude(transform.position - playerPos);
        if (distance >= chaseDetectionDistance * chaseDetectionDistance)
        {
            navmeshAgent.destination = transform.position;
            state = State.Idle;
            OnStateChange?.Invoke(state);
        }
        //If player in shooting range then try to shoot
        else if (distance <= shootDistance * shootDistance)
        {
            Shooting();
        }

      
    }

    private void Shooting()
    {
        if (attackCooldownTimer <= 0f)
        {
            //Get playerpos and check if player is in a certain cone in front of tank using angles
            //If player is in cone launch attack directly from the tank attackPoint
            Vector3 playerPos = Player.Instance.CharacterInstance.playerBehaviourTree.modelTransform.position;
            Vector3 playerDir = playerPos - transform.position;
            playerDir.Normalize();
            //Get angle between playerDir and transform.forward
            float dot = Vector3.Dot(playerDir, transform.forward);

            Debug.DrawLine(transform.position, transform.position+playerDir, Color.red, 1f);
            print(dot);
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
        if (attackCooldownTimer <= 0f)
        {
            state = State.Chasing;
            OnStateChange?.Invoke(state);
        }
    }
    public float GetHealthNormalized()
    {
        return (float)currentHealth / (float)maxHealth;
    }
}
