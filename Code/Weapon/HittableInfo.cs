using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HittableInfo
{
    public float shield = 100;
    public float health = 100;

    public float ProceedHealth(float currnetHealth, float damage)
    {
        return currnetHealth - damage;
    }
    
    public float ProceedShieldDamage(float damage, float currentShield, out float healthDamage)
    {
        healthDamage = 0;
        if (damage > currentShield)
        {
            healthDamage = damage - currentShield;
            currentShield = 0;
        }
        else
        {
            currentShield -= damage;
        }
        return currentShield;
    }
}
