// ------------------------------------------------------
// 역할:
//   - 서버에서 호출하면 모든 클라이언트에서 Animator 트리거/플래그를 동일하게 재생.
// 전제:
//   - 같은 오브젝트(또는 자식)에 Animator가 있어야 한다.
// 규칙 준수.
using UnityEngine;
using Unity.Netcode;

//[RequireComponent(typeof(Animator))]
public class PlayerAnimationRelay : NetworkBehaviour
{
    public Animator anim;

    void Awake()
    {
        //anim = GetComponent<Animator>();
    }

    // ------- 서버에서 호출하는 공개 메서드 -------
    public void ServerPlayFire()
    {
        if (IsServer == true)
        {
            PlayFireClientRpc();
        }
    }

    public void ServerPlayJump()
    {
        if (IsServer == true)
        {
            PlayJumpClientRpc();
        }
    }

    public void ServerPlayDie()
    {
        if (IsServer == true)
        {
            PlayDieClientRpc();
        }
    }

    public void ServerPlayRespawn()
    {
        if (IsServer == true)
        {
            PlayRespawnClientRpc();
        }
    }

    // ------- 모든 클라에서 실행되는 RPC -------
    [ClientRpc]
    private void PlayFireClientRpc(ClientRpcParams p = default)
    {
        if (anim != null)
        {
            //anim.ResetTrigger("Fire");
            //anim.SetTrigger("Fire");
        }
    }

    [ClientRpc]
    private void PlayJumpClientRpc(ClientRpcParams p = default)
    {
        if (anim != null)
        {
            //anim.ResetTrigger("Jump");
            //anim.SetTrigger("Jump");
        }
    }

    [ClientRpc]
    private void PlayDieClientRpc(ClientRpcParams p = default)
    {
        if (anim != null)
        {
            //anim.ResetTrigger("Die");
            //anim.SetTrigger("Die");
        }
    }

    [ClientRpc]
    private void PlayRespawnClientRpc(ClientRpcParams p = default)
    {
        if (anim != null)
        {
            //anim.ResetTrigger("Respawn");
            //anim.SetTrigger("Respawn");
        }
    }
}
