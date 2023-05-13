using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private LayerMask _layerMask;
    private Camera _mainCamera;
    private Inventory _inventory;
    private bool _isLocked = false;

    private void Awake()
    {
        _mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Confined;
        _inventory = FindObjectOfType<Inventory>();
        _inventory.OnOpenInventory += LockCamera;
        _inventory.OnCloseInventory += UnlockCamera;
    }

    // void Update()
    // {
    //     var mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
    //     var lookDirection = (mousePos - transform.position).normalized;
    //     _mainCamera.transform.rotation = Quaternion.LookRotation(lookDirection, transform.up);
    // }
    //
    // private void OnDrawGizmos()
    // {
    //     var mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawLine(transform.position, mousePos);
    // }
    
    [SerializeField] private float _rotationSpeed = 6.0f;
    [SerializeField] private float _moveSpeed = 6.0f;
    [SerializeField] private float _interactDistance = 20.0f;
    [SerializeField] private LayerMask _interactMask;

    private float _xRotation = 0.0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) Interact();
        
        // TODO: allow rotating camera while locked by moving cursor into the corner of the screen
        if (_isLocked) return;
        
        Look();
        Move();

        // if (Input.GetMouseButtonDown(0))
        // {
        //     RaycastHit hit;
        //     var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        //     if (Physics.Raycast(ray, out hit, 10f, _layerMask))
        //     {
        //         var cell = hit.collider.GetComponentInParent<InventoryCell>();
        //         // cell?.OnPointerEnter();
        //     }
        // }
    }

    void GrabItem()
    {
        // TODO
    }

    void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, _interactDistance, _interactMask))
        {
            var interactable = hit.transform.gameObject.GetComponentInParent<IInteractable>();

            interactable?.Interact();
        }
    }

    void Look()
    {
        var horizontal = Input.GetAxis("Mouse X") * _rotationSpeed * Time.deltaTime;
        var vertical = Input.GetAxis("Mouse Y") * _rotationSpeed * Time.deltaTime;

        _xRotation -= vertical;
        _xRotation = Mathf.Clamp(_xRotation, -90.0f, 90.0f);

        transform.localRotation = Quaternion.Euler(_xRotation, transform.localEulerAngles.y + horizontal, 0);
    }

    void Move()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        
        var move = _moveSpeed * Time.deltaTime * (x * transform.right + y * transform.forward);
        transform.position += move;
    }

    void LockCamera()
    {
        _isLocked = true;
    }

    void UnlockCamera()
    {
        _isLocked = false;
    }

    private void OnDisable()
    {
        _inventory.OnOpenInventory -= LockCamera;
        _inventory.OnCloseInventory -= UnlockCamera;
    }
}
