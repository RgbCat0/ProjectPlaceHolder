public interface IDamageable
{
    float Health { get; }
    void TakeDamageRpc(float damage);
}