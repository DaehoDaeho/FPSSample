using UnityEngine;

public class DamageTest : MonoBehaviour
{
    public PlayerStatus player;
    public Weapon weapon;

    void Awake()
    {
        if (!player) player = FindObjectOfType<PlayerStatus>();
        if (!weapon) weapon = FindObjectOfType<Weapon>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) player.TakeDamage(25f); // ������
        if (Input.GetKeyDown(KeyCode.J)) player.Heal(25f);       // ȸ��
        //if (Input.GetMouseButtonDown(0)) weapon.Shoot();         // ���
        if (Input.GetKeyDown(KeyCode.R)) weapon.Reload();        // ������
    }
}
