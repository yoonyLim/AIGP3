using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CooldownUI : MonoBehaviour
{
    [Header("링 채우기용 이미지")]
    [SerializeField] Image ringFill;   // Ring_Fill
    [Header("쿨다운 숫자(선택)")]
    [SerializeField] TextMeshProUGUI timeText;

    float _remain;     // 남은 시간
    float _duration;   // 전체 시간
    bool _running;

    // 외부에서 호출
    public void Begin(float seconds)
    {
        _duration = seconds;
        _remain = seconds;
        _running = true;
        ringFill.fillAmount = 1f;      // 가득
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
