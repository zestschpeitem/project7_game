using UnityEngine;

public class CharacterComponent : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    public HealthComponent HealthComponent { get => healthComponent; }

    [SerializeField] private AttackComponent attackComponent;
    public AttackComponent AttackComponent { get => attackComponent; }

    private HealthComponent targetHealthComponent;
    public enum State
    {
        Idle,
        RunningToEnemy,
        RunningFromEnemy,
        ZombieRunningToEnemy,
        BeginAttack,
        Attack,
        BeginShoot,
        Shoot,
        Death,
    }

    public enum Weapon
    {
        Pistol,
        Bat,
        Kulak,
    }

    Animator animator;
    State state;

    public Weapon weapon;
    public float runSpeed;
    public float distanceFromEnemy;

    Vector3 originalPosition;
    Quaternion originalRotation;



    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        state = State.Idle;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void SetState(State newState)
    {
        state = newState;
    }

    public void SetTarget(HealthComponent target)
    {
        this.targetHealthComponent = target;
    }

    public void StartTurn()
    {
        if (healthComponent.IsDead)
        {
            //TODO: rework logic
            attackComponent.OnAttackFinished?.Invoke(); // bad approach fix it
            // probably need to locate such logic in some gifferent pace
            return;
        }

        AttackEnemy();
    }

    [ContextMenu("Attack")]
    void AttackEnemy()
    {
        switch (weapon)
        {
            case Weapon.Bat:
                state = State.RunningToEnemy;
                break;
            case Weapon.Pistol:
                state = State.BeginShoot;
                break;
            case Weapon.Kulak:
                state = State.ZombieRunningToEnemy;
                break;
        }
    }

    bool RunTowards(Vector3 targetPosition, float distanceFromTarget)
    {
        Vector3 distance = targetPosition - transform.position;
        if (distance.magnitude < 0.00001f)
        {
            transform.position = targetPosition;
            return true;
        }

        Vector3 direction = distance.normalized;
        transform.rotation = Quaternion.LookRotation(direction);

        targetPosition -= direction * distanceFromTarget;
        distance = targetPosition - transform.position;

        Vector3 step = direction * runSpeed;
        if (step.magnitude < distance.magnitude)
        {
            transform.position += step;
            return false;
        }

        transform.position = targetPosition;
        return true;
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Idle:
                transform.rotation = originalRotation;
                animator.SetFloat("Speed", 0.0f);
                break;

            case State.RunningToEnemy:
                animator.SetFloat("Speed", runSpeed);
                if (RunTowards(targetHealthComponent.transform.position, distanceFromEnemy))
                    state = State.BeginAttack;
                break;

            case State.RunningFromEnemy:
                animator.SetFloat("Speed", runSpeed);
                if (RunTowards(originalPosition, 0.0f))
                {
                    state = State.Idle;
                    AttackFinished();
                }
                break;

            case State.ZombieRunningToEnemy:
                animator.SetFloat("ZombieSpeed", runSpeed);
                if (RunTowards(targetHealthComponent.transform.position, distanceFromEnemy))
                    state = State.BeginAttack;
                break;

            case State.BeginAttack:
                animator.SetTrigger("MeleeAttack");
                state = State.Attack;
                break;

            case State.Attack:
                if(weapon == Weapon.Kulak)
                {
                    animator.SetFloat("ZombieSpeed", 0.0f);
                }
                break;

            case State.BeginShoot:
                animator.SetTrigger("Shoot");
                state = State.Shoot;
                break;

            case State.Shoot:
                break;

            case State.Death:
                animator.SetTrigger("Death");
                break;

        }
    }
    public void AttackFinished()
    {
        attackComponent.Attack(targetHealthComponent);
    }
    private void CharacterDeath()
    {
        state = State.Death;
    }

    private void OnEnable()
    {
        healthComponent.OnDead += CharacterDeath;
    }

    private void OnDisable()
    {
        healthComponent.OnDead -= CharacterDeath;
    }
}
