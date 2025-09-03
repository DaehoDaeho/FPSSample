// ------------------------------------------------------
// ����:
//   - �������� ȣ���ϸ� ��� Ŭ���̾�Ʈ���� Animator Ʈ����/�÷��׸� �����ϰ� ���.
// ����:
//   - ���� ������Ʈ(�Ǵ� �ڽ�)�� Animator�� �־�� �Ѵ�.
// ��Ģ �ؼ�.
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

    // ------- �������� ȣ���ϴ� ���� �޼��� -------
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

    // ------- ��� Ŭ�󿡼� ����Ǵ� RPC -------
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
