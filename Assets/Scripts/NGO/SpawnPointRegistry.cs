// SpawnPointRegistry.cs
// -----------------------------------------------
// 역할:
//   - 팀 A/B의 스폰 위치 목록을 보관하고, "다음 스폰 위치"를 순환으로 반환.
//   - 네트워크 오브젝트가 아닌, 단순 씬 매니저 오브젝트.
// 사용법:
//   - 씬에 빈 오브젝트를 만들고 이 스크립트를 붙인다.
//   - 인스펙터에서 teamASpawns, teamBSpawns 배열에 Transform을 채운다.
// -----------------------------------------------
using UnityEngine;

public class SpawnPointRegistry : MonoBehaviour
{
    public Transform[] teamASpawns;  // 팀 A 스폰 지점 목록
    public Transform[] teamBSpawns;  // 팀 B 스폰 지점 목록

    private int teamAIndex = 0;      // A팀 다음 스폰 인덱스(순환)
    private int teamBIndex = 0;      // B팀 다음 스폰 인덱스(순환)

    // team == 0 이면 A팀, 1이면 B팀
    public Vector3 GetNextSpawnPosition(int team)
    {
        if (team == 0)
        {
            // A팀 처리
            if (teamASpawns != null)
            {
                if (teamASpawns.Length > 0)
                {
                    Transform t = teamASpawns[teamAIndex % teamASpawns.Length];
                    teamAIndex = teamAIndex + 1;

                    if (t != null)
                    {
                        return t.position; // 유효한 Transform이면 그 위치 반환
                    }
                }
            }
        }
        else
        {
            // B팀 처리
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

        // 설정이 비었거나 문제가 있으면 (0,0,0) 반환
        return Vector3.zero;
    }
}
