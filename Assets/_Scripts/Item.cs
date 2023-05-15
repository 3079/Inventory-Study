using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    // [SerializeField] public int _inventoryWidth { get; private set; }
    // [SerializeField] public int _inventoryHeight { get; private set; }
    public int _inventoryWidth;
    public int _inventoryHeight;
    [SerializeField] private float _inventoryScaleFactor;

    public void OnGrabbed()
    {
        // transform.rotation = Quaternion.identity;
    }
    public void OnEnterInventorySpace()
    {
        transform.rotation = Quaternion.identity;
        var scale = transform.localScale;
        scale *= _inventoryScaleFactor;
        transform.localScale = scale;
    }
    
    public void OnExitInventorySpace()
    {
        var scale = transform.localScale;
        scale /= _inventoryScaleFactor;
        transform.localScale = scale;
    }
}
