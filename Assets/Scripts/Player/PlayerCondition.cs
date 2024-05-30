using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    void TakePhysicalDamage(int damage);
}

public class PlayerCondition : MonoBehaviour, IDamagable
{
    public UICondition uiCondition;
    public TimerUI timerUI;

    Condition health { get { return uiCondition.health; } }
    Condition hunger { get { return uiCondition.hunger; } }
    Condition stamina { get { return uiCondition.stamina; } }

    public float noHungerHealthDecay;

    public event Action onTakeDamage;

    private Rigidbody rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        hunger.Subtract(hunger.passiveValue * Time.deltaTime);
        stamina.Add(stamina.passiveValue * Time.deltaTime);

        if (hunger.curValue == 0f)
        {
            health.Subtract(noHungerHealthDecay * Time.deltaTime);
        }

        if (health.curValue == 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        health.Add(amount);
    }

    public void Eat(float amount)
    {
        hunger.Add(amount);
    }

    public void SpeedUp(float amount, float duration)
    {
        StartCoroutine(TemporarySpeedUp(amount, duration));
    }

    private IEnumerator TemporarySpeedUp(float amount, float duration)
    {
        CharacterManager.Instance.Player.controller.SetMoveSpeed(CharacterManager.Instance.Player.controller.originalMoveSpeed + amount);
        timerUI.StartTimer(duration);
        yield return new WaitForSeconds(duration);
        CharacterManager.Instance.Player.controller.SetMoveSpeed(CharacterManager.Instance.Player.controller.originalMoveSpeed - amount);
    }

    public void IncreaseJumpCount(float amount, float duration)
    {
        StartCoroutine(TemporaryIncreaseJumpCount(amount, duration));
    }

    private IEnumerator TemporaryIncreaseJumpCount(float amount, float duration)
    {
        CharacterManager.Instance.Player.controller.maxJumpCount += (int)amount;
        timerUI.StartTimer(duration);
        yield return new WaitForSeconds(duration);
        CharacterManager.Instance.Player.controller.maxJumpCount -= (int)amount;
    }

    public void IncreaseJumpPower(float amount, float duration)
    {
        StartCoroutine(TemporaryIncreaseJumpPower(amount, duration));
    }

    private IEnumerator TemporaryIncreaseJumpPower(float amount, float duration)
    {
        CharacterManager.Instance.Player.controller.jumpForce += amount;
        timerUI.StartTimer(duration);
        yield return new WaitForSeconds(duration);
        CharacterManager.Instance.Player.controller.jumpForce -= amount;
    }

    public void Die()
    {
        Debug.Log("DIE");
    }

    public void TakePhysicalDamage(int damage)
    {
        health.Subtract(damage);
        onTakeDamage?.Invoke();
        if (rigidbody != null)
        {
            Vector3 velocity = rigidbody.velocity;
            velocity.y = Mathf.Clamp(velocity.y, 0, float.MaxValue); // 수직 속도를 제한
            rigidbody.velocity = velocity;
        }
    }

    public bool UseStamina(float amount)
    {
        if (stamina.curValue - amount < 0)
        {
            return false;
        }
        stamina.Subtract(amount);
        return true;
    }
}
