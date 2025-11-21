using UnityEngine;

public interface IDamagable
{
    // Nesnenin hasar almasını sağlar ve canını azaltır.
    /// <param name="damage">Alınan hasar miktarı.</param>
    void TakeDamage(float damage);

    // Nesnenin hayatta olup olmadığını gösteren salt okunur özellik.
    
    bool IsAlive { get; }
}