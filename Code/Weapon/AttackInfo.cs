[System.Serializable]
public class AttackInfo
{
    public float damage = 50f;
    public float attackSpeed = 0.1f;
    public float headShotMultiplier = 1.5f;

    public float ProceedDamage(bool critical)
    {
        return critical ? damage * headShotMultiplier : damage;
    }
}
