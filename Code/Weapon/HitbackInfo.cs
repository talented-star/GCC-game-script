using System;

[Serializable]
public class HitbackInfo
{
    public BodyPart bodyPart;
    public ClassHitTarget classHitTarget;
    public float damage;
    public float remainingHealth;
    public int grantValue;
    public bool isLastHit;
    //public IAttackable attackable;

    private HitbackInfo() { }

    public HitbackInfo(BodyPart bodyPart, float damage, float remainingHealth)
    {
        this.bodyPart = bodyPart;
        this.damage = damage;
        this.remainingHealth = remainingHealth;
        //this.isLastHit = isLastHit;
        //this.attackable = attackable;
    }

    public static HitbackInfo Deserialize(byte[] data)
    {
        HitbackInfo result = new();

        result.bodyPart = (BodyPart)data[0];
        result.classHitTarget = (ClassHitTarget)data[1];
        result.damage = (float)BitConverter.Int64BitsToDouble(BitConverter.ToInt64(data, sizeof(byte) * 2));
        result.remainingHealth = (float)BitConverter.Int64BitsToDouble(BitConverter.ToInt64(data, sizeof(byte) * 2 + sizeof(double)));
        result.grantValue = BitConverter.ToInt32(data, sizeof(byte) * 2 + sizeof(double) * 2);
        result.isLastHit = BitConverter.ToBoolean(data, sizeof(byte) * 2 + sizeof(double) * 2 + sizeof(int));

        return result;
    }

    public byte[] Serialize()
    {
        byte[] result = new byte[
            (sizeof(byte) * 2) + 
            (sizeof(double) * 2) +
            (sizeof(int) * 2) +
            sizeof(bool)
            ];

        BitConverter.GetBytes((byte)bodyPart).CopyTo(result, 0);
        BitConverter.GetBytes((byte)classHitTarget).CopyTo(result, sizeof(byte));
        BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(damage)).CopyTo(result, sizeof(byte) * 2);
        BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(remainingHealth)).CopyTo(result, sizeof(byte) * 2 + sizeof(double));
        BitConverter.GetBytes(grantValue).CopyTo(result, sizeof(byte) * 2 + sizeof(double) * 2);
        BitConverter.GetBytes(isLastHit).CopyTo(result, sizeof(byte) * 2 + sizeof(double) * 2 + sizeof(int));

        return result;
    }
}
