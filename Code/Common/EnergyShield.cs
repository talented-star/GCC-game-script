using PlayFab.ClientModels;
using PlayFabCatalog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnergyShield : MonoBehaviour
{
    [SerializeField] private MeshRenderer _shieldMaterial;
    [SerializeField] private float _timeFlash = 0.2f;

    private float _timerFlash;
    private bool _isFlash;

    private ShieldCustomData _shieldCustomData;
    private float _timeout;
    private float _currentCapacity;
    private bool _isBoostRecharge;
    private bool _isBatteryInject;

    public Action<float> onRegenTick;
    public float CurrentCapacity => _currentCapacity;

    internal void Init(ShieldCustomData shieldCustomData, float currentCapacity)
    {
        _shieldCustomData = shieldCustomData;
        _currentCapacity = currentCapacity;
    }

    private void Update()
    {
        if (_shieldCustomData == null) return;
        if (_timeout > 0)
            _timeout -= Time.deltaTime;
        else if (_currentCapacity < (_isBatteryInject ? _shieldCustomData.capacity : _shieldCustomData.threshold))
        {
            _currentCapacity += _shieldCustomData.regenSpeed * Time.deltaTime * (_isBoostRecharge ? 2f : 1f);
            onRegenTick?.Invoke(_currentCapacity);
        }
    }

    public async void Flash()
    {
        _isFlash = true;
        //Debug.Log($"Flash!");
        _timerFlash = 0;
        while (_timerFlash <= _timeFlash)
        {
            var v = Mathf.Sin(_timerFlash / _timeFlash * 180f * Mathf.Deg2Rad);

            float value = (_timerFlash / (_timeFlash / 2f)) % 1f;
            _shieldMaterial.material.SetFloat("_GeneralAlpha", v);
            await Task.Delay(20);
            _timerFlash += 0.02f;
        }
        _isFlash = false;
        _timerFlash = 0;
    }

    public void EnergyControl(ref float damage)
    {
        _timeout = _shieldCustomData.timeout;
        if (_currentCapacity <= 0) return;

        if (_currentCapacity < damage)
        {
            damage -= _currentCapacity;
            _currentCapacity = 0;
        }
        else
        {
            _currentCapacity = _currentCapacity - damage;
            damage = 0;
        }
    }

    public async void InjectBattery()
    {
        _isBatteryInject = true;
        if (_timeout > 0)
            _isBoostRecharge = true;
        await Task.Delay(20000);
        _isBoostRecharge = false;
        _isBatteryInject = false;
    }

    //public bool PushFlash;
    //private void OnValidate()
    //{
    //    if (PushFlash)
    //    {
    //        PushFlash = false;
    //        Flash();
    //    }
    //}
}
