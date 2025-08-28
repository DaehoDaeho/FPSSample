// NetworkColorName.cs
// ----------------------------------------------------
// 역할:
//   - OwnerClientId를 이용해 고유한 색을 정하고, 캡슐 머리 위에 이름표를 띄운다.
// ----------------------------------------------------

using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class NetworkColorName : NetworkBehaviour
{
    public Renderer bodyRenderer; // 캡슐 MeshRenderer
    public TMP_Text worldNameText;    // 월드 공간 Text (Player 자식 Canvas의 Text)

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
