using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int hp = 100;

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log("�÷��̾ ������� �Ծ����ϴ�. ���� HP : " + hp);

        if(hp <= 0)
        {
            Debug.Log("�ñ��!!!");
            // ���� ���� ó�� �ϱ�. �� ��ȯ or ���� ���� UI ���.
        }
    }
}
