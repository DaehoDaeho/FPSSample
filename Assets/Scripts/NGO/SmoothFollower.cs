// ------------------------------------------------------
// ����:
//   - Ŭ���̾�Ʈ������ "���� ��ġ/ȸ��"�� ���� ���� Transform�� �ε巴�� �����.
//   - ����/ȣ��Ʈ������ �ƹ��͵� ���� �ʴ´�.
// ����:
//   - Player �����տ� ����. NetworkTransform(���� ����)�� �Բ� ���.
// ------------------------------------------------------
using UnityEngine;
using Unity.Netcode;

public class SmoothFollower : NetworkBehaviour
{
    public float positionLerp = 12.0f; // ���� Ŭ���� ���� ����.
    public float rotationLerp = 12.0f;

    void LateUpdate()
    {
        // ����/ȣ��Ʈ���� ���� �����̴� ���� ���� ���ʿ�(�Ǵ� ��ġ ����)
        if (IsServer == true)
        {
            return;
        }

        // ��Ʈ��ũ�� ���� "��¥ Transform"�� �����θ� ��¦ �����ؼ� �����.
        Vector3 targetPos = transform.position;      // NetworkTransform�� �̹� ����ȭ�� ��.
        Quaternion targetRot = transform.rotation;   // ������ �Ϻ� ��Ÿ��/ī�޶� ���� ������ ����.

        // ������ �ڱ� �ڽ��� ��ġ�� �ٽ� ���� �� �ƴ϶�,
        // "ī�޶� �� �ڽ� ������Ʈ" ���ؿ��� ������ ���̴� �뵵���.
        // ������ �� �ڽĿ� �� ��ũ��Ʈ�� �ް� �ð� ��Ʈ�� �����ϴ� ���� �������̴�.
        // (������ ������ Transform ������ �ÿ�)
    }
}
