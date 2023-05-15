using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 6.0f;
    [SerializeField] private float _moveSpeed = 6.0f;
    [SerializeField] private float _interactDistance = 20.0f;
    [SerializeField] private LayerMask _interactMask;
    [SerializeField] private LayerMask _itemMask;
    
    private float _xRotation = 0.0f;
    private Camera _mainCamera;
    private Inventory _inventory;
    private bool _isLocked = false;
    
    private GameObject _heldItem;
    private bool _holdingItem;
    private List<InventoryCell> _hoveredCells = new List<InventoryCell>();

    // required for item-canvas interactions
    private GraphicRaycaster _raycaster;
    private EventSystem _eventSystem;
    private PointerEventData _pointer;
    private List<Vector3> points;

    private void Awake()
    {
        _mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Confined;
        _inventory = FindObjectOfType<Inventory>();
        _inventory.OnOpenInventory += LockCamera;
        _inventory.OnCloseInventory += UnlockCamera;
        _raycaster = FindObjectOfType<GraphicRaycaster>();
        _eventSystem = FindObjectOfType<EventSystem>();
        // _pointer = new PointerEventData(_eventSystem);
        // _pointer.hovered = new List<GameObject>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) Interact();
        
        // TODO: allow rotating camera while locked by moving cursor into the corner of the screen
        if (_isLocked)
        {
            // UpdatePointerPosition()
            // {
            // }

            if (_holdingItem)
            {
                var distanceFromCamera = Vector3.Dot(_heldItem.transform.position - _mainCamera.transform.position, _mainCamera.transform.forward);

                var mousePositionInWorld = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceFromCamera));
                _heldItem.transform.position = mousePositionInWorld;
                
                HoveredCells();
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                if (_holdingItem)
                {
                    PlaceItem();
                }
                else
                {
                    RaycastHit hit;
                    var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, 10f, _itemMask))
                    {
                        var hitObject = hit.transform.gameObject;
                        var item = hitObject.GetComponent<Item>();
                        if (item != null) GrabItem(hitObject, item);
                    }
                }
            }
        }
        else
        {
            Look();
            Move();
        }

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

    private void UpdatePointerPosition()
    {
        if (!_holdingItem) return;
        PointerInputModule input = _eventSystem.currentInputModule as PointerInputModule;
    }

    private void GrabItem(GameObject itemObj, Item item)
    {
        // TODO
        if (_holdingItem) return;
        itemObj.transform.parent = transform;
        item.OnGrabbed();
        _heldItem = item.gameObject;
        _holdingItem = true;
        HoveredCells();
        foreach (var cell in _hoveredCells)
        {
            cell.GrabItem();
        }
        // Debug.Log("item width: " + item._inventoryWidth);
        // Debug.Log("item height: " + item._inventoryHeight);
    }
    
    private bool PlaceItem()
    {
        // TODO
        // var cells = HoveredCells();
        var pos = Vector3.zero;
        var item = _heldItem.GetComponent<Item>();
        // Debug.Log("Trying To Place Item");
        // Debug.Log("Hovered cells: " + _hoveredCells.Count);
        if (_hoveredCells.Count < item._inventoryWidth * item._inventoryHeight) return false;
        
        foreach (var cell in _hoveredCells)
        {
            if (!cell.IsEmpty()) return false;
            cell.StoreItem(item);
            pos += _inventory._grid.ObjectWorldPosition(cell);
        }

        pos /= _hoveredCells.Count;
        _heldItem.transform.position = pos;
        _heldItem.transform.parent = null;
        _heldItem = null;
        _holdingItem = false;
        return true;
    }

    // private List<InventoryCell> HoveredCells()
    // {
    //     var item = _heldItem.GetComponent<Item>();
    //     List<InventoryCell> hitCells = new List<InventoryCell>();
    //     
    //     for (int x = 0; x < item._inventoryWidth; x++)
    //     {
    //         for (int y = 0; y < item._inventoryHeight; y++)
    //         {
    //             // calculate center point coordinates of each cell that the item occupies and cast a ray from it into the inventory
    //             var xDelta = item._inventoryWidth == 1 ? 1 : item._inventoryWidth / (item._inventoryWidth - 1);
    //             var yDelta = item._inventoryHeight == 1 ? 1 : item._inventoryHeight / (item._inventoryHeight - 1);
    //             
    //             var point = item.transform.position +
    //                         _inventory.CellSize * (x * xDelta - item._inventoryWidth * 0.5f) * _inventory.transform.right +
    //                         _inventory.CellSize * (y * yDelta - item._inventoryHeight * 0.5f) * _inventory.transform.up;
    //             
    //             var pointer = new PointerEventData(_eventSystem);
    //             pointer.position = point;
    //             pointer.hovered = new List<GameObject>();
    //             List<RaycastResult> results = new List<RaycastResult>();
    //             // _raycaster.Raycast(_pointer, result);
    //             _eventSystem.RaycastAll(pointer, results);
    //             
    //             // detect all hit cells and accumulate them in hitCells list
    //             
    //             // var filtered = result.Select(m => m.gameObject.GetComponent<IPointerEnterHandler>()).Where(m => m != null).ToList();
    //             // foreach (var pointerEnterHandler in filtered)
    //             // {
    //             //     _pointer.hovered = new List<GameObject>() {pointerEnterHandler};
    //             //     pointerEnterHandler.OnPointerEnter(_pointer);
    //             // }
    //             Debug.Log("Raycast hit " + results.Count + " objects");
    //
    //             List<InventoryCell> hoveredCells;
    //
    //             foreach (var result in results)
    //             {
    //                 var hitObject = result.gameObject;
    //                 var cell = hitObject.GetComponent<InventoryCell>();
    //                 
    //                 if (cell == null) continue;
    //                 
    //                 if (_hoveredCells.Contains(cell)) continue;
    //                 
    //                 hitCells.Add(cell);
    //                 pointer.hovered.Add(hitObject);
    //                 // TODO: hit cells can't track whether the pointer has exited or any other callbacks
    //                 cell.OnPointerEnter(pointer);
    //             }
    //             // hitCells.AddRange(filtered);
    //         }
    //     }
    //
    //     Debug.Log("Hovering over " + hitCells.Count + " cells");
    //     return hitCells;
    // }

    private void HoveredCells()
    {
        //debug
        points = new List<Vector3>();
        
        var item = _heldItem.GetComponent<Item>();
        var hoveredCells = new List<InventoryCell>();
        
        for (int x = 0; x < item._inventoryWidth; x++)
        {
            for (int y = 0; y < item._inventoryHeight; y++)
            {
                // calculate center point coordinates of each cell that the item occupies and cast a ray from it into the inventory
                // var xDelta = item._inventoryWidth == 1 ? 1 : item._inventoryWidth / (item._inventoryWidth - 1);
                // var yDelta = item._inventoryHeight == 1 ? 1 : item._inventoryHeight / (item._inventoryHeight - 1);

                var offset = _inventory.CellSize * (x - (item._inventoryWidth - 1) * 0.5f) * _inventory.GridParent.right +
                             _inventory.CellSize * (y - (item._inventoryHeight - 1) * 0.5f) * _inventory.GridParent.up;
                var point = item.transform.position + offset;

                //debug
                points.Add(point);
                // Debug.Log("Offset X: " + (x - (item._inventoryWidth == 1 ? 0 : item._inventoryWidth * 0.5f)));
                // Debug.Log("Offset Y: " + (y - (item._inventoryHeight == 1 ? 0 : item._inventoryHeight * 0.5f)));

                var pointer = new PointerEventData(_eventSystem);
                pointer.position = point;
                pointer.hovered = new List<GameObject>();
                
                var cell = _inventory.GetCellAtWorldPos(point);
                if(cell != null)
                    hoveredCells.Add(cell);
                // cell?.OnPointerEnter(pointer);
            }
        }

        var newCells = hoveredCells.Except(_hoveredCells);
        var removedCells = _hoveredCells.Except(hoveredCells);

        foreach (var cell in newCells)
        {
            cell?.SetColor(InventoryCell.ColorType.AVAILABLE);
        }
        
        foreach (var cell in removedCells)
        {
            cell?.ResetCell();
        }

        _hoveredCells = hoveredCells;
    }

    private void OnDrawGizmos()
    {
        if(!_holdingItem) return;
        
        Gizmos.color = Color.green;
        foreach (var point in points)
        {
            Gizmos.DrawLine(point, point + _inventory.GridParent.forward * 1f);
        }
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
