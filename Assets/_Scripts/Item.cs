using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    // [SerializeField] public int _inventoryWidth { get; private set; }
    // [SerializeField] public int _inventoryHeight { get; private set; }
    [Header("General Item Parameters")]
    public int _inventoryWidth;
    public int _inventoryHeight;
    public int _inventorySize => _inventoryHeight * _inventoryWidth;
    [SerializeField] private float _inventoryScaleFactor;
    private Collider _collider;
    // public float LocalZAngle { get; private set; } = 0f;
    public int LocalZAngle = 0;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    public void OnGrabbed()
    {
        // or maybe just set different layers and layerMasks for UI elements and items
    }
    
    public void OnDropped()
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
        // transform.rotation = Quaternion.identity;
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

    public void Rotate(Vector3 axis)
    {
        // TODO DEBUG
        axis = axis == Vector3.zero ? -1 * transform.forward : axis;
        // axis = -1 * transform.forward;
        axis = transform.parent ? transform.parent.InverseTransformDirection(axis) : axis;
        var rotation = Quaternion.AngleAxis(90f, axis) *  transform.localRotation;
        LocalZAngle = (LocalZAngle + 270) % 360;
        transform.SetLocalPositionAndRotation(transform.localPosition, rotation);
        (_inventoryWidth, _inventoryHeight) = (_inventoryHeight, _inventoryWidth);
    }
}
