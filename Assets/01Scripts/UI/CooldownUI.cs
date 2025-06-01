using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CooldownUI : MonoBehaviour
{
    [Header("�� ä���� �̹���")]
    [SerializeField] Image ringFill;   // Ring_Fill
    [Header("��ٿ� ����(����)")]
    [SerializeField] TextMeshProUGUI timeText;

    float _remain;     // ���� �ð�
    float _duration;   // ��ü �ð�
    bool _running;

    // �ܺο��� ȣ��
    public void Begin(float seconds)
    {
        _duration = seconds;
        _remain = seconds;
        _running = true;
        ringFill.fillAmount = 1f;      // ����
        if (timeText) timeText.enabled = true;
    }

    void Update()
    {
        if (!_running) return;

        _remain -= Time.deltaTime;
        float t = Mathf.Clamp01(_remain / _duration);
        ringFill.fillAmount = t;

        if (timeText)
        {
            timeText.text = Mathf.CeilToInt(_remain).ToString();
            if (_remain <= 0.1f) timeText.enabled = false;
        }

        if (_remain <= 0f) _running = false;
    }
}
