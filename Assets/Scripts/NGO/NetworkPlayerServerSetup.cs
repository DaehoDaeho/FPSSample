// NetworkPlayerServerSetup.cs
// ------------------------------------------------------
// 역할:
//   - 서버가 각 플레이어에게 팀을 배정(간단히 접속 순서로 0/1 번갈아)한다.
//   - 배정된 팀의 스폰 위치로 플레이어를 배치한다.
//   - Owner(본인)에게만 카메라를 붙여 1인칭 시야를 만든다.
// 의존:
//   - 씬에 SpawnPointRegistry가 있어야 한다.
//   - Player 프리팹에 CameraMount, PitchPivot이 있어야 한다.
// 권한:
//   - 팀 정보는 NetworkVariable<int>로 모든 클라가 읽고, 서버만 쓴다.
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerServerSetup : NetworkBehaviour
{
    public Transform cameraMount;   // 카메라가 붙을 위치(머리)
    public Transform pitchPivot;    // 상하 회전 피벗(마우스 룩용)

    // Everyone(누구나 읽기), Server(서버만 쓰기)
    public NetworkVariable<int> team =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        // 서버가 팀 배정 및 스폰 위치 설정
        if (IsServer == true)
        {
            // 간단 규칙: 소유 클라이언트 ID의 홀짝에 따라 팀 배정
            int assigned = 0;
            if (OwnerClientId % 2 == 1)
            {
                assigned = 1;
            }
            team.Value = assigned;

            // 스폰 포인트 찾기
            SpawnPointRegistry reg = GameObject.FindObjectOfType<SpawnPointRegistry>();
            if (reg != null)
            {
                Vector3 pos = reg.GetNextSpawnPosition(team.Value);
                // CharacterController가 있다면 순간이동 시 충돌을 피하기 위해
                // 사망/리스폰 로직에서 controller.enabled를 조절한다(아래 스크립트 참고).
                transform.position = pos;
            }
        }

        // Owner(본인)에게만 카메라를 붙임
        if (IsOwner == true)
        {
            Camera cam = Camera.main;

            if (cam != null)
            {
                // PitchPivot이 있으면 거기에 붙여 상하 회전이 카메라에 반영되도록 한다.
                Transform target = pitchPivot;
                if (target == null)
                {
                    target = cameraMount;
                }

                if (target != null)
                {
                    cam.transform.SetParent(target);
                    // 로컬 위치/회전을 0으로 맞춰 정확히 장착
                    cam.transform.localPosition = Vector3.zero;
                    cam.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }
}
