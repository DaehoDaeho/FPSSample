using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform player;
    public GameObject enemyPrefab;  // �� �������� ������ ����.
    public Transform[] spawnPoints; // ���� ��ġ�� ������ �迭.
    public float spawnInterval = 2.0f;
    public int enemiesPerWave = 5;
    int spawned = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartWave()
    {
        spawned = 0;

        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        while(spawned < enemiesPerWave)
        {
            int idx = Random.Range(0, spawnPoints.Length);
            GameObject obj = Instantiate(enemyPrefab, spawnPoints[idx].position, Quaternion.identity);
            EnemyAI enemyAI = obj.GetComponent<EnemyAI>();
            if(enemyAI != null)
            {
                enemyAI.SetTarget(player);
            }
            ++spawned;
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
