// OwnerNetworkTransform.cs
// 역할: NetworkTransform을 "클라이언트(오너) 권한"으로 동작시키기
using Unity.Netcode;
using Unity.Netcode.Components;

public class OwnerNetworkTransform : NetworkTransform
{
    // 이 오브젝트의 Transform 권한을 서버가 아니라 "오너 클라이언트"에게 준다.
    // NetworkTransform은 이 오버라이드를 통해 권한을 결정한다.
    protected override bool OnIsServerAuthoritative()
    {
        return false; // false = Client(Owner) authoritative
    }
}
