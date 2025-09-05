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
    public CharacterController characterController;

    private bool deathRMActive = false;
    private float orgHeight;
    private Vector3 orgCenter;

    public float deadHeight = 0.6f;
    public float deadCenterY = 0.3f;

    void Awake()
    {
        //anim = GetComponent<Animator>();

        orgHeight = characterController.height;
        orgCenter = characterController.center;
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
            anim.ResetTrigger("Fire");
            anim.SetTrigger("Fire");
        }
    }

    [ClientRpc]
    private void PlayJumpClientRpc(ClientRpcParams p = default)
    {
        if (anim != null)
        {
            anim.ResetTrigger("Jump");
            anim.SetTrigger("Jump");
        }

        // 같은 오브젝트(메쉬 루트)에 붙어 있으니 바로 찾기.
        PlayerAnimationLocomotion loco = GetComponent<PlayerAnimationLocomotion>();
        if (loco != null)
        {
            loco.OnJumpTriggeredLocal();
        }
    }

    [ClientRpc]
    private void PlayDieClientRpc(ClientRpcParams p = default)
    {
        if (anim != null)
        {
            deathRMActive = true;

            characterController.height = deadHeight;
            characterController.center = new Vector3(orgCenter.x, deadCenterY, orgCenter.z);

            anim.applyRootMotion = true;
            anim.ResetTrigger("Die");
            anim.SetTrigger("Die");
        }
    }

    [ClientRpc]
    private void PlayRespawnClientRpc(ClientRpcParams p = default)
    {
        if (anim != null)
        {
            deathRMActive = false;

            anim.applyRootMotion = false;
            anim.ResetTrigger("Respawn");
            anim.SetTrigger("Respawn");

            characterController.height = orgHeight;
            characterController.center = orgCenter;
        }
    }
}
