public interface IDamageable
{
    public void ApplyDamage(int amt, DamageReason damageReason, ulong instigator = 0);
}
