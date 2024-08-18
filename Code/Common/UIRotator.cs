using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRotator : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private Vector3 _direction = Vector3.forward;

    void Update()
    {
        transform.Rotate(_direction, Time.deltaTime * _speed);
    }
}
