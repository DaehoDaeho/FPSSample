// NameBillboard.cs
// -----------------------------------------------------
// ����: �̸�ǥ(���� ���� Canvas/Text)�� �׻� ���� ī�޶� ���ϵ��� ȸ��.
// Ư¡:
//   - yawOnly==true �̸� �¿�(Yaw)�� ���߰� ���ϴ� ���� ����(��鸲 ����).
//   - distanceScaling==true �̸� �Ÿ� ���� ũ�� ����(�ʹ� �ָ� �۾����� ���� ��ȭ).
//   - hideWhenBehind==true �̸� ī�޶� �ڿ� ���� �� �ڵ� ����.
// ��Ʈ��ũ:
//   - ���� "����"������ ó��. ȸ�� ����ȭ ���ʿ�(Ʈ���� 0).
// -----------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

public class NameBillboard : MonoBehaviour
{
    [Header("Facing")]
    public bool yawOnly = true;                  // true: ���� ����(����), false: ����(Pitch)���� ���� ����
    public Transform targetCameraTransform;      // ����θ� �ڵ� �Ҵ�(Camera.main)

    [Header("Visibility")]
    public bool hideWhenBehind = true;           // ī�޶� �����̸� ����
    public float behindDotThreshold = 0.0f;      // 0���� ������ ���� ��

    [Header("Distance Scaling")]
    public bool distanceScaling = true;          // �Ÿ� ����
    public float nearDistance = 2.0f;            // �� �Ÿ����� ������ maxScale
    public float farDistance = 20.0f;           // �� �Ÿ����� �ָ� minScale
    public float maxScale = 1.0f;            // ����� �� ������
    public float minScale = 0.6f;            // �� �� ������

    // ����
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

        // 1) �׻� ī�޶� ���� ���� ȸ��
        Vector3 toCamera = transform.position - camTr.position;

        if (yawOnly == true)
        {
            toCamera.y = 0.0f; // ���� �����(���ϴ� ����)
        }

        if (toCamera.sqrMagnitude > 0.000001f)
        {
            Quaternion look = Quaternion.LookRotation(toCamera, Vector3.up);
            transform.rotation = look;
        }

        // 2) ī�޶� �ڿ� ������ ����(�ɼ�)
        if (hideWhenBehind == true)
        {
            // ī�޶� �ٶ󺸴� ����� "ī�޶�->�̸�ǥ" ������ ����
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

        // 3) �Ÿ� ������ ����(�ɼ�)
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
        // Text, Image, CanvasGroup �� ����
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
