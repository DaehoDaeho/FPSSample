// NetworkColorName.cs
// ----------------------------------------------------
// ����:
//   - OwnerClientId�� �̿��� ������ ���� ���ϰ�, ĸ�� �Ӹ� ���� �̸�ǥ�� ����.
// ----------------------------------------------------

using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class NetworkColorName : NetworkBehaviour
{
    public Renderer bodyRenderer; // ĸ�� MeshRenderer
    public TMP_Text worldNameText;    // ���� ���� Text (Player �ڽ� Canvas�� Text)

    public override void OnNetworkSpawn()
    {
        ApplyDeterministicColor();
        ApplyNameTag();
    }

    private void ApplyDeterministicColor()
    {
        float h = (OwnerClientId % 10) * 0.1f; // 0.0, 0.1, 0.2 ...
        Color c = Color.HSVToRGB(h, 0.6f, 0.9f);

        if (bodyRenderer != null)
        {
            if (bodyRenderer.material != null)
            {
                bodyRenderer.material.color = c;
            }
        }
    }

    private void ApplyNameTag()
    {
        string who = "Client " + OwnerClientId.ToString();

        if (IsOwner == true)
        {
            who = who + " (Me)";
        }

        if (worldNameText != null)
        {
            worldNameText.text = who;
        }
    }
}
