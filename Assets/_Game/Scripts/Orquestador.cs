using UnityEngine;

public class Orquestador : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public EnemyPoblator enemyPoblator;
    public GridObjectSpawner[] gridObjectSpawners;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapGenerator.GenerateMap();
        enemyPoblator.Iniciar();
        for (int i = 0; i < gridObjectSpawners.Length; i++)
        {
            gridObjectSpawners[i].Iniciar();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
