using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int hp = 100;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log("플레이어가 대미지를 입었습니다. 현재 HP : " + hp);

        if(hp <= 0)
        {
            Debug.Log("꼴까닥!!!");
            // 게임 오버 처리 하기. 씬 전환 or 게임 오버 UI 출력.
        }
    }
}
