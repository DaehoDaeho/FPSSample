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

        // ���� ������Ʈ(�޽� ��Ʈ)�� �پ� ������ �ٷ� ã��.
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
