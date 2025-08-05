using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public EnemySpawner spawner;
    public int wave = 1;
    public int enemiesLeft;
    public TextMeshProUGUI textScore;
    int myScore = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartNextWave();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartNextWave()
    {
        spawner.enemiesPerWave = 5 * wave;  // ���̵� ������ ���� �� ���̺꿡 �ش�Ǵ� ���� ���� ����.
        spawner.StartWave();
        enemiesLeft = spawner.enemiesPerWave;
    }

    public void OnEnemyDefeated()
    {
        --enemiesLeft;
        if(enemiesLeft <= 0)
        {
            ++wave;
            StartNextWave();
        }
    }

    public void AddScore(int score)
    {
        myScore += score;
        textScore.text = myScore.ToString();
    }
}
