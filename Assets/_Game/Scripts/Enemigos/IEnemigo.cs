using UnityEngine;

/// <summary>
/// Interfaz Principal de los enemigos
/// </summary>
public interface IEnemigo
{
    // Atributos básicos
    float Vida { get; set; }
    float Velocidad { get; set; }
    float Damage { get; set; }

    // Acciones comunes
    void RecibirDamage(float cantidad);
    void Atacar(GameObject objetivo);
}
