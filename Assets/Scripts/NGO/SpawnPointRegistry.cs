// SpawnPointRegistry.cs
// -----------------------------------------------
// ����:
//   - �� A/B�� ���� ��ġ ����� �����ϰ�, "���� ���� ��ġ"�� ��ȯ���� ��ȯ.
//   - ��Ʈ��ũ ������Ʈ�� �ƴ�, �ܼ� �� �Ŵ��� ������Ʈ.
// ����:
//   - ���� �� ������Ʈ�� ����� �� ��ũ��Ʈ�� ���δ�.
//   - �ν����Ϳ��� teamASpawns, teamBSpawns �迭�� Transform�� ä���.
// -----------------------------------------------
using UnityEngine;

public class SpawnPointRegistry : MonoBehaviour
{
    public Transform[] teamASpawns;  // �� A ���� ���� ���
    public Transform[] teamBSpawns;  // �� B ���� ���� ���

    private int teamAIndex = 0;      // A�� ���� ���� �ε���(��ȯ)
    private int teamBIndex = 0;      // B�� ���� ���� �ε���(��ȯ)

    // team == 0 �̸� A��, 1�̸� B��
    public Vector3 GetNextSpawnPosition(int team)
    {
        if (team == 0)
        {
            // A�� ó��
            if (teamASpawns != null)
            {
                if (teamASpawns.Length > 0)
                {
                    Transform t = teamASpawns[teamAIndex % teamASpawns.Length];
                    teamAIndex = teamAIndex + 1;

                    if (t != null)
                    {
                        return t.position; // ��ȿ�� Transform�̸� �� ��ġ ��ȯ
                    }
                }
            }
        }
        else
        {
            // B�� ó��
            if (teamBSpawns != null)
            {
                if (teamBSpawns.Length > 0)
                {
                    Transform t = teamBSpawns[teamBIndex % teamBSpawns.Length];
                    teamBIndex = teamBIndex + 1;

                    if (t != null)
                    {
                        return t.position;
                    }
                }
            }
        }

        // ������ ����ų� ������ ������ (0,0,0) ��ȯ
        return Vector3.zero;
    }
}
