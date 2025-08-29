// NameBillboard.cs
// -----------------------------------------------------
// 목적: 이름표(월드 공간 Canvas/Text)가 항상 로컬 카메라를 향하도록 회전.
// 특징:
//   - yawOnly==true 이면 좌우(Yaw)만 맞추고 상하는 수평 유지(흔들림 방지).
//   - distanceScaling==true 이면 거리 따라 크기 보정(너무 멀면 작아지는 현상 완화).
//   - hideWhenBehind==true 이면 카메라 뒤에 있을 때 자동 숨김.
// 네트워크:
//   - 전부 "로컬"에서만 처리. 회전 동기화 불필요(트래픽 0).
// -----------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

public class NameBillboard : MonoBehaviour
{
    [Header("Facing")]
    public bool yawOnly = true;                  // true: 수평만 맞춤(권장), false: 상하(Pitch)까지 완전 정면
    public Transform targetCameraTransform;      // 비워두면 자동 할당(Camera.main)

    [Header("Visibility")]
    public bool hideWhenBehind = true;           // 카메라 뒤쪽이면 숨김
    public float behindDotThreshold = 0.0f;      // 0보다 작으면 거의 뒤

    [Header("Distance Scaling")]
    public bool distanceScaling = true;          // 거리 보정
    public float nearDistance = 2.0f;            // 이 거리보다 가까우면 maxScale
    public float farDistance = 20.0f;           // 이 거리보다 멀면 minScale
    public float maxScale = 1.0f;            // 가까울 때 스케일
    public float minScale = 0.6f;            // 멀 때 스케일

    // 내부
    private Camera cachedCam;
    private Transform camTr;
    private Vector3 initialLocalScale;

    void Awake()
    {
        initialLocalScale = transform.localScale;
    }

    void Start()
    {
        TryCacheCamera();
    }

    void LateUpdate()
    {
        if (camTr == null)
        {
            TryCacheCamera();
            if (camTr == null)
            {
                return;
            }
        }

        // 1) 항상 카메라 정면 보게 회전
        Vector3 toCamera = transform.position - camTr.position;

        if (yawOnly == true)
        {
            toCamera.y = 0.0f; // 수평만 맞춘다(상하는 고정)
        }

        if (toCamera.sqrMagnitude > 0.000001f)
        {
            Quaternion look = Quaternion.LookRotation(toCamera, Vector3.up);
            transform.rotation = look;
        }

        // 2) 카메라 뒤에 있으면 숨김(옵션)
        if (hideWhenBehind == true)
        {
            // 카메라가 바라보는 방향과 "카메라->이름표" 방향의 내적
            Vector3 camForward = camTr.forward;
            Vector3 camToName = (transform.position - camTr.position).normalized;
            float dot = Vector3.Dot(camForward, camToName);

            bool behind = false;
            if (dot < behindDotThreshold)
            {
                behind = true;
            }

            SetGraphicEnabled(enabledState: (behind == false));
        }

        // 3) 거리 스케일 보정(옵션)
        if (distanceScaling == true)
        {
            float d = Vector3.Distance(camTr.position, transform.position);
            float t = 0.0f;

            if (farDistance > nearDistance)
            {
                t = (d - nearDistance) / (farDistance - nearDistance);
            }

            if (t < 0.0f)
            {
                t = 0.0f;
            }
            if (t > 1.0f)
            {
                t = 1.0f;
            }

            float s = Mathf.Lerp(maxScale, minScale, t);
            Vector3 baseScale = initialLocalScale;
            transform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, baseScale.z * s);
        }
    }

    private void TryCacheCamera()
    {
        if (targetCameraTransform != null)
        {
            camTr = targetCameraTransform;
            return;
        }

        if (cachedCam == null)
        {
            cachedCam = Camera.main;
        }

        if (cachedCam != null)
        {
            camTr = cachedCam.transform;
        }
    }

    private void SetGraphicEnabled(bool enabledState)
    {
        // Text, Image, CanvasGroup 등 지원
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = enabledState ? 1.0f : 0.0f;
        }

        Graphic g = GetComponentInChildren<Graphic>();
        if (g != null)
        {
            g.enabled = enabledState;
        }
    }
}
