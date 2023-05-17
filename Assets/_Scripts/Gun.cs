using System;
using UnityEngine;

public class Gun : Item
{
    [Header("Gun Parameters")]
    [SerializeField] private int _capacity;
    [SerializeField] private int _ammo;
}
