using System;
using Components;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterComponent : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private TargetIndicatorComponent targetIndicatorComponent;
    public HealthComponent HealthComponent { get => healthComponent; }

    [SerializeField] private AttackComponent attackComponent;
    public AttackComponent AttackComponent { get => attackComponent; }

    public TargetIndicatorComponent IndicatorComponent => targetIndicatorComponent;

    private HealthComponent targetHealthComponent;

    public enum State
    {
        Idle,
        RunningToEnemy,
        RunningFromEnemy,
        BeginAttack,
        Attack,
        BeginShoot,
        Shoot,
    }

    Animator animator;
    State state;

    public WeaponData _weapon;
    public float runSpeed;
    public float distanceFromEnemy;
    Vector3 originalPosition;
    Quaternion originalRotation;
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int zSpeed = Animator.StringToHash("zSpeed");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int MeleeAttack = Animator.StringToHash("MeleeAttack");
    private static readonly int ZombieAttack = Animator.StringToHash("ZombieAttack");
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        state = State.Idle;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        healthComponent.OnDead += OnDead;
    }

    private void OnDead()
    {
        animator.SetTrigger(Death);
    }

    private void OnDestroy()
    {
        healthComponent.OnDead -= OnDead;
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
        switch (_weapon.Weapon)
        {
            case Weapon.Bat:
                state = State.RunningToEnemy;
                break;
            case Weapon.Pistol:
                state = State.BeginShoot;
                break;
            case Weapon.Kulak:
                state = State.RunningToEnemy;
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
                animator.SetFloat(Speed, 0.0f);
                animator.SetFloat(zSpeed, 0.0f);
                break;

            case State.RunningToEnemy:
                if (_weapon.Weapon == Weapon.Bat)
                {
                    animator.SetFloat(Speed, runSpeed);
                }

                if (_weapon.Weapon == Weapon.Kulak)
                {
                    animator.SetFloat(zSpeed, runSpeed);
                }

                if (RunTowards(targetHealthComponent.transform.position, distanceFromEnemy))
                    state = State.BeginAttack;
                break;

            case State.RunningFromEnemy:
                if (_weapon.Weapon == Weapon.Bat)
                {
                    animator.SetFloat(Speed, runSpeed);
                }

                if (_weapon.Weapon == Weapon.Kulak)
                {
                    animator.SetFloat(zSpeed, runSpeed);
                }

                if (RunTowards(originalPosition, 0.0f))
                {
                    state = State.Idle;
                    AttackFinished();
                }
                break;

            case State.BeginAttack:
                if (_weapon.Weapon == Weapon.Bat)
                {
                    animator.SetTrigger(MeleeAttack);
                    state = State.Attack;
                }

                if (_weapon.Weapon == Weapon.Kulak)
                {
                    animator.SetTrigger(ZombieAttack);
                    state = State.Attack;
                }
                break;

            case State.Attack:
                break;

            case State.BeginShoot:
                animator.SetTrigger(Shoot);
                state = State.Shoot;
                break;

            case State.Shoot:
                break;
        }
    }
    public void AttackFinished()
    {
        attackComponent.Attack(targetHealthComponent);
    }


}