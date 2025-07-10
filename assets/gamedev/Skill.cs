using UnityEngine;

public abstract class Skill : ScriptableObject
{
    public string skillID;
    public string skillName;
    public float skillPower;
    public float cooldownTime;
    public AnimationClip skillAnimation;

    private bool isOnCooldown = false;
    private float currentCooldown = 0f;
    public bool isActive = false;

    public virtual bool CanActivate()
    {
        return !isOnCooldown && !isActive;
    }

    public void UpdateCooldown(float deltaTime)
    {
        if (isOnCooldown)
        {
            currentCooldown -= deltaTime;
            Debug.Log($"Cooldown running: {currentCooldown}s left");
            if (currentCooldown <= 0f)
            {
                isOnCooldown = false;
                Debug.Log("Cooldown ended");
            }
        }
    }

    public void StartCooldown()
    {
        isOnCooldown = true;
        currentCooldown = cooldownTime;
    }

    public abstract void ActivateSkill(GameObject user);
}
