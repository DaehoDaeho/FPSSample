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
        if (Input.GetKeyDown(KeyCode.H)) player.TakeDamage(25f); // 데미지
        if (Input.GetKeyDown(KeyCode.J)) player.Heal(25f);       // 회복
        //if (Input.GetMouseButtonDown(0)) weapon.Shoot();         // 사격
        if (Input.GetKeyDown(KeyCode.R)) weapon.Reload();        // 재장전
    }
}
