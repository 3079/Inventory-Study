using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    // [SerializeField] public int _inventoryWidth { get; private set; }
    // [SerializeField] public int _inventoryHeight { get; private set; }
    [Header("General Item Parameters")]
    public int _inventoryWidth;
    public int _inventoryHeight;
    [SerializeField] private float _inventoryScaleFactor;
    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    public void OnGrabbed()
    {
        // or maybe just set different layers and layerMasks for UI elements and items
    }

    public void OnInventoryEnter()
    {
        // TODO implement that callback in cameraController
        _collider.enabled = false;
    }

    public void OnInventoryExit()
    {
        // TODO implement that callback in cameraController
        _collider.enabled = true;
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

    public void OnCallContextMenu()
    {
        return;
    }
}
