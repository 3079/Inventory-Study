using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    // [SerializeField] public int _inventoryWidth { get; private set; }
    // [SerializeField] public int _inventoryHeight { get; private set; }
    public int _inventoryWidth;
    public int _inventoryHeight;
    [SerializeField] private float _inventoryScaleFactor;

    void OnEnterInventorySpace()
    {
        transform.rotation = Quaternion.identity;
        var scale = transform.localScale;
        scale *= _inventoryScaleFactor;
        transform.localScale = scale;
    }
    
    void OnExitInventorySpace()
    {
        var scale = transform.localScale;
        scale /= _inventoryScaleFactor;
        transform.localScale = scale;
    }
}
