using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class HealthComponent : MonoBehaviour
{
    public Action<int> OnHealthChanged;

    public Action OnDead;

    public int Health
    {
        get => health;
    }

    public bool IsDead
    {
        get => isDead;
    }

    [SerializeField]
    private int health;

    [SerializeField]
    private bool isDead;

    [SerializeField]
    private GameObject damageEffect;

    [SerializeField]
    private PlaySound playSound;

    [SerializeField]
    private List<string> playSoundNames = new List<string>();

    public void ApplyDamage(int damage)
    {
        StartCoroutine(DamageSoundCoroutine(damage));

        if (health <= 0)
        {
            isDead = true;
            health = 0;
            OnDead?.Invoke();
            StartCoroutine(DeathSoundCoroutine());
        }

        OnHealthChanged?.Invoke(health);
    }

    private void ShowDamageEffect()
    {
        if (!damageEffect)
        {
            return;
        }

        foreach (var effect in damageEffect.GetComponentsInChildren<ParticleSystem>())
        {
            effect.Play();
        }
    }

    private IEnumerator DamageSoundCoroutine(int damage)
    {
        health -= damage;

        yield return new WaitForSeconds(0.2f);

        ShowDamageEffect();
        if (playSound)
        {
            playSound.PlaySoundEffect(playSoundNames[0]);
        }
    }

    private IEnumerator DeathSoundCoroutine()
    {
        yield return new WaitForSeconds(0.2f);

        if (playSound)
        {
            playSound.PlaySoundEffect(playSoundNames[1]);
        }
    }
}