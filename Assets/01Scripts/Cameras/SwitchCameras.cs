using System;
using Unity.Cinemachine;
using UnityEngine;
using Random = System.Random;

public class SwitchCameras : MonoBehaviour
{
    [SerializeField] private float cameraSwitchInterval = 10f;
    [SerializeField] private float cameraSwitchInvtervalVariance = 3f;
    [SerializeField] CinemachineCamera attackerCamera;
    [SerializeField] CinemachineCamera defenderCamera;

    private bool _isAttackCameraEnabled = true;
    private float _nextInterval;
    private float _elapsedTime = 0;

    private void Start()
    {
        defenderCamera.enabled = false;
        _nextInterval = cameraSwitchInterval + UnityEngine.Random.Range(-cameraSwitchInvtervalVariance, cameraSwitchInvtervalVariance);
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _nextInterval)
        {
            attackerCamera.enabled = !_isAttackCameraEnabled;
            defenderCamera.enabled = _isAttackCameraEnabled;
            _isAttackCameraEnabled = !_isAttackCameraEnabled;
            
            _nextInterval = cameraSwitchInterval + UnityEngine.Random.Range(-cameraSwitchInvtervalVariance, cameraSwitchInvtervalVariance);
            _elapsedTime = 0;
        }
    }
}
