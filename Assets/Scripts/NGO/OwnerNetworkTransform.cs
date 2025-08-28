// OwnerNetworkTransform.cs
// ����: NetworkTransform�� "Ŭ���̾�Ʈ(����) ����"���� ���۽�Ű��
using Unity.Netcode;
using Unity.Netcode.Components;

public class OwnerNetworkTransform : NetworkTransform
{
    // �� ������Ʈ�� Transform ������ ������ �ƴ϶� "���� Ŭ���̾�Ʈ"���� �ش�.
    // NetworkTransform�� �� �������̵带 ���� ������ �����Ѵ�.
    protected override bool OnIsServerAuthoritative()
    {
        return false; // false = Client(Owner) authoritative
    }
}
