// PlayerWeaponController_OO.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerWeaponController_OO : MonoBehaviour
{
    [Header("Weapons")]
    public WeaponBase[] weapons;
    public int startIndex = 0;

    [Header("UI (Optional)")]
    public TextMeshProUGUI weaponNameText;

    private int currentIndex = -1;

    void Start()
    {
        int count = weapons == null ? 0 : weapons.Length;

        // 전부 끄기
        for (int i = 0; i < count; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].OnUnequip();
            }
        }

        // 시작 무기 장착
        if (count > 0)
        {
            EquipIndex(startIndex);
        }
    }

    void Update()
    {
        // 발사(왼쪽 마우스)
        if (Input.GetButton("Fire1") == true)
        {
            if (currentIndex >= 0)
            {
                if (weapons[currentIndex] != null)
                {
                    weapons[currentIndex].Fire();
                }
            }
        }

        // 무기 교체: 숫자 키
        if (Input.GetKeyDown(KeyCode.Alpha1) == true) { EquipIndex(0); }
        if (Input.GetKeyDown(KeyCode.Alpha2) == true) { EquipIndex(1); }
        if (Input.GetKeyDown(KeyCode.Alpha3) == true) { EquipIndex(2); }

        // 마우스 휠: 위(다음), 아래(이전)
        float wheel = Input.GetAxis("Mouse ScrollWheel");
        if (wheel > 0f)
        {
            NextWeapon();
        }
        else
        {
            if (wheel < 0f)
            {
                PrevWeapon();
            }
        }

        // 선택 무기 틱 업데이트(필요 시)
        if (currentIndex >= 0)
        {
            if (weapons[currentIndex] != null)
            {
                weapons[currentIndex].TickUpdate();
            }
        }
    }

    void EquipIndex(int index)
    {
        int count = weapons == null ? 0 : weapons.Length;

        if (index < 0 || index >= count)
        {
            return;
        }

        if (currentIndex == index)
        {
            return;
        }

        // 기존 해제
        if (currentIndex >= 0)
        {
            if (weapons[currentIndex] != null)
            {
                weapons[currentIndex].OnUnequip();
            }
        }

        // 새 무기 장착
        currentIndex = index;

        if (weapons[currentIndex] != null)
        {
            weapons[currentIndex].OnEquip();

            if (weaponNameText != null)
            {
                weaponNameText.text = weapons[currentIndex].weaponName;
            }
        }
    }

    void NextWeapon()
    {
        int count = weapons == null ? 0 : weapons.Length;

        if (count <= 0)
        {
            return;
        }

        int next = currentIndex + 1;
        if (next >= count)
        {
            next = 0;
        }
        EquipIndex(next);
    }

    void PrevWeapon()
    {
        int count = weapons == null ? 0 : weapons.Length;

        if (count <= 0)
        {
            return;
        }

        int prev = currentIndex - 1;
        if (prev < 0)
        {
            prev = count - 1;
        }
        EquipIndex(prev);
    }
}
