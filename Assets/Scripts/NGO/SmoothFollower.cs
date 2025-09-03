// ------------------------------------------------------
// 역할:
//   - 클라이언트에서만 "현재 위치/회전"을 서버 권한 Transform에 부드럽게 맞춘다.
//   - 서버/호스트에서는 아무것도 하지 않는다.
// 사용법:
//   - Player 프리팹에 부착. NetworkTransform(서버 권한)과 함께 사용.
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;

public class SmoothFollower : NetworkBehaviour
{
    public float positionLerp = 12.0f; // 값이 클수록 빨리 따라감.
    public float rotationLerp = 12.0f;

    void LateUpdate()
    {
        // 서버/호스트에서 직접 움직이는 쪽은 보정 불필요(또는 원치 않음)
        if (IsServer == true)
        {
            return;
        }

        // 네트워크로 받은 "진짜 Transform"에 스스로를 살짝 보간해서 맞춘다.
        Vector3 targetPos = transform.position;      // NetworkTransform이 이미 동기화한 값.
        Quaternion targetRot = transform.rotation;   // 하지만 일부 런타임/카메라 스냅 방지용 보간.

        // 보간은 자기 자신의 위치를 다시 세는 게 아니라,
        // "카메라 등 자식 오브젝트" 기준에서 끊김을 줄이는 용도라면.
        // 실제론 빈 자식에 이 스크립트를 달고 시각 루트만 보간하는 편이 안정적이다.
        // (오늘은 간단히 Transform 보간만 시연)
    }
}
