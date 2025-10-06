using UnityEngine;

/// <summary>
/// Clase base abstracta para no repetir codigo
/// </summary>
public abstract class EnemigoBase : MonoBehaviour, IEnemigo
{
    [Header("Stats del Enemigo")]
    public float vida = 100f;
    public float velocidad = 3f;
    public float damage = 10f;

    public float Vida { get => vida; set => vida = value; }
    public float Velocidad { get => velocidad; set => velocidad = value; }
    public float Damage { get => damage; set => damage = value; }

    /// <summary>
    /// Metodo global para recibir daño
    /// </summary>
    /// <param name="cantidad"> Cantidad de daño aplicado al enemigo</param>
    public virtual void RecibirDamage(float cantidad)
    {
        vida -= cantidad;
        Debug.Log($"{gameObject.name} recibió {cantidad} de daño. Vida restante: {vida}");

        if (vida <= 0) Morir();
    }

    /// <summary>
    /// Metodo global para el ataque
    /// </summary>
    /// <param name="objetivo"> Objeto a atacar, normalmente el jugador</param>
    public abstract void Atacar(GameObject objetivo);

    /// <summary>
    /// Metodo global invocado al momento de destruir el enemigo
    /// </summary>
    protected virtual void Morir()
    {
        Debug.Log($"{gameObject.name} murió.");
        Destroy(gameObject);
    }
}
