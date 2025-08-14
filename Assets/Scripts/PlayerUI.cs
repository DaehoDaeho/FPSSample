using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public PlayerStatus playerStatus;
    public Weapon weapon;

    public Slider hpBar;
    public TextMeshProUGUI ammoText;
    public Image damageOverlay;

    public float hpsmoothSpeed = 5.0f;  // 부드러운 보간 처리를 위한 변수.
    private float hpTarget;
    private float hpCurrent;

    public Color ammoNormalColor = Color.white; // 탄약이 보통 상태의 개수일 때.
    public Color ammoEmptyColor = Color.red;    // 탄약이 비었을 때
    public float ammoBlinkInterval = 0.25f; // 깜박거리는 시간 간격.
    private Coroutine ammoBlinkRoutine;

    public float hitFlashAlpha = 0.35f; // 피격 시 화면 깜박임 이미지의 투명 값.
    public float hitFadeDuration = 0.25f;
    private Coroutine hitFlashRoutine;

    private void Awake()
    {
        if(playerStatus == null)
        {
            playerStatus = FindObjectOfType<PlayerStatus>();
        }

        if(weapon == null)
        {
            weapon = FindObjectOfType<Weapon>();
        }
    }

    private void OnEnable()
    {
        if(playerStatus != null)
        {
            playerStatus.OnHpChanged += HandleHpChanged;
        }

        if(weapon != null)
        {
            weapon.OnAmmoChanged += HandleAmmoChanged;
        }
    }

    private void OnDisable()
    {
        if (playerStatus != null)
        {
            playerStatus.OnHpChanged -= HandleHpChanged;
        }

        if(weapon != null)
        {
            weapon.OnAmmoChanged -= HandleAmmoChanged;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(playerStatus != null)
        {
            HandleHpChanged(playerStatus.CurrentHP, playerStatus.maxHP);
        }
    }

    // Update is called once per frame
    void Update()
    {
        hpCurrent = Mathf.Lerp(hpCurrent, hpTarget, Time.deltaTime * hpsmoothSpeed);
        hpBar.value = hpCurrent;
    }

    private void HandleHpChanged(float current, float max)
    {
        if(max <= 0)
        {
            hpTarget = 0.0f;
        }
        else
        {
            hpTarget = current / max;
        }

        hpBar.fillRect.GetComponent<Image>().color = HPColorFromRatio(hpTarget);

        if(hitFlashRoutine != null)
        {
            StopCoroutine(hitFlashRoutine);
        }
        hitFlashRoutine = StartCoroutine(HitFlash());
    }

    private void HandleAmmoChanged(int current, int reserve)
    {
        ammoText.text = current.ToString() + " / " + reserve.ToString();
        bool isEmpty = (current == 0);

        if(isEmpty == true)
        {
            if(ammoBlinkRoutine == null)
            {
                ammoBlinkRoutine = StartCoroutine(AmmoBlink());
            }
            else
            {
                if(ammoBlinkRoutine != null)
                {
                    StopCoroutine(ammoBlinkRoutine);
                    ammoBlinkRoutine = null;
                }

                ammoText.color = ammoNormalColor;
            }
        }
    }

    private IEnumerator AmmoBlink()
    {
        while(true)
        {
            ammoText.color = ammoEmptyColor;
            yield return new WaitForSeconds(ammoBlinkInterval);
            ammoText.color = ammoNormalColor;
            yield return new WaitForSeconds(ammoBlinkInterval);
        }
    }


    private IEnumerator HitFlash()
    {
        if(damageOverlay == null)
        {
            yield break;
        }

        damageOverlay.color = new Color(1.0f, 0.0f, 0.0f, hitFlashAlpha);

        float t = 0.0f;
        while(t < hitFadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(hitFlashAlpha, 0.0f, t / hitFadeDuration);
            damageOverlay.color = new Color(1.0f, 0.0f, 0.0f, a);
            yield return null;
        }

        damageOverlay.color = new Color(1.0f, 0.0f, 0.0f, 0.0f);
    }

    private Color HPColorFromRatio(float r)
    {
        if(r >= 0.5f)
        {
            float t = Mathf.InverseLerp(0.5f, 1.0f, r);
            return Color.Lerp(Color.yellow, Color.green, t);
        }
        else
        {
            float t = Mathf.InverseLerp(0.0f, 0.5f, r);
            return Color.Lerp(Color.red, Color.yellow, t);
        }
    }
}
