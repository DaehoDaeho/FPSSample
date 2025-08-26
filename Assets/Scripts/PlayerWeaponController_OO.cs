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

        // ���� ����
        for (int i = 0; i < count; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].OnUnequip();
            }
        }

        // ���� ���� ����
        if (count > 0)
        {
            EquipIndex(startIndex);
        }
    }

    void Update()
    {
        // �߻�(���� ���콺)
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

        // ���� ��ü: ���� Ű
        if (Input.GetKeyDown(KeyCode.Alpha1) == true) { EquipIndex(0); }
        if (Input.GetKeyDown(KeyCode.Alpha2) == true) { EquipIndex(1); }
        if (Input.GetKeyDown(KeyCode.Alpha3) == true) { EquipIndex(2); }

        // ���콺 ��: ��(����), �Ʒ�(����)
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

        // ���� ���� ƽ ������Ʈ(�ʿ� ��)
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

        // ���� ����
        if (currentIndex >= 0)
        {
            if (weapons[currentIndex] != null)
            {
                weapons[currentIndex].OnUnequip();
            }
        }

        // �� ���� ����
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
