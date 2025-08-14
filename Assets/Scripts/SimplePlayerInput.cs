using UnityEngine;

public class SimplePlayerInput : MonoBehaviour
{
    public Weapon weapon;

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.CurrentState != GameManager.GameState.Playing)
            return; // �Ͻ�����/���ӿ��� �� �Է� ����

        if (Input.GetMouseButtonDown(0)) weapon.Shoot();
        if (Input.GetKeyDown(KeyCode.R)) weapon.Reload();
    }
}
