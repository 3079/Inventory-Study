using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private bool _isInventoryOpen = false;
    
    private GameObject _heldItem = null;
    private bool _isHoldingItem = false;
    private List<InventoryCell> _hoveredCells = new List<InventoryCell>();

    // required for item-canvas interactions
    private GraphicRaycaster _raycaster;
    private EventSystem _eventSystem;
    private PointerEventData _pointer;
    // for debugging purposes
    private List<Vector3> points;

    private void Awake()
    {
        _mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Confined;
        _inventory = FindObjectOfType<Inventory>();
        _inventory.OnOpenInventory += OnOpenInventory;
        _inventory.OnCloseInventory += OnCloseInventory;
        _raycaster = FindObjectOfType<GraphicRaycaster>();
        _eventSystem = FindObjectOfType<EventSystem>();
        // _pointer = new PointerEventData(_eventSystem);
        // _pointer.hovered = new List<GameObject>();
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.E)) Interact();
        if (Input.GetKeyDown(KeyCode.Tab)) _inventory.Interact();

        var isCursorOverInventory = IsCursorOverInventory();
        
        HandleMouseMovement(_isInventoryOpen, _isHoldingItem, isCursorOverInventory);
        if(Input.GetMouseButtonDown(0)) HandleLeftClick(_isInventoryOpen, _isHoldingItem, isCursorOverInventory, Vector3.zero);
        if(Input.GetMouseButtonDown(1)) HandleRightClick();
        
        
        // TODO: allow rotating camera while locked by moving cursor into the corner of the screen
        // if (_isInventoryOpen)
        // {
        //     if (_isHoldingItem)
        //     {
        //         _heldItem.transform.position = MousePositionInWorld();
        //         
        //         HoveredCells();
        //     }
        //
        //     if (Input.GetMouseButtonDown(1))
        //     {
        //         if(_isHoldingItem) RotateItem();
        //     }
        //
        //     // LMB Behaviour
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         // if (_holdingItem)
        //         // {
        //         //     if (IsCursorOverInventory())
        //         //     {
        //         //         // this behaviour is handled in InventoryCell class
        //         //     }
        //         //     else
        //         //     {
        //         //         DropItem();
        //         //     }
        //         // }
        //         // else
        //         // {
        //         //     if (IsCursorOverInventory())
        //         //     {
        //         //         // this behaviour is handled in InventoryCell class
        //         //     }
        //         //     else
        //         //     {
        //         //         var item = GrabItemRaycast();
        //         //         if (item != null) GrabItem(item.gameObject, item);
        //         //     }
        //         // }
        //
        //         
        //         // TODO there's a bug when item is snapped to the grid but the mouse is outside of it and you press LMB item doesn't get stored
        //         
        //         // inverted version of the commented code above
        //         if (!isCursorOverInventory)
        //         {
        //             if (_isHoldingItem)
        //             {
        //                 // TODO
        //                 if (IsItemOverInventory()) PlaceItem();
        //                 else DropItem();
        //             }
        //             else
        //             {
        //                 var item = GrabItemRaycast();
        //                 if (item != null) GrabItem(item.gameObject, item);
        //             }
        //         }
        //     }
        //     
        //     // TODO Add item dropping based on surface normal
        //     
        //     // TODO Potentially add better state control for when in/out inventory UI, hovering UI / hovering world surface, holding / not holding item
        // }
        // else
        // {
        //     Look();
        //     Move();
        // }

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

    private void HandleMouseMovement(bool isInventoryOpen, bool isHoldingItem, bool isCursorOverInventory)
    {
        if (isInventoryOpen)
        {
            if (isHoldingItem)
            {
                _heldItem.transform.position = MousePositionInWorld();
                HoveredCells();

                if (isCursorOverInventory)
                {
                    
                }
                else
                {
                    // TODO: Raycast for item position
                }
            }
            else
            {
                // TODO: Check for screen corners and rotate camera if needed
            }
        }
        else
        {
            Look();
            Move();
            
            if (isHoldingItem)
            {
                // TODO: Move object to screen center with a small force, so that it can return back if put off center via colliding
            }
        }
    }
    
    private void HandleLeftClick(bool isInventoryOpen, bool isHoldingItem, bool isCursorOverInventory, Vector3 targetedSurface)
    {
        if (isInventoryOpen)
        {
            if (isCursorOverInventory) return;
            
            if (isHoldingItem)
            {
                if (IsItemOverInventory()) PlaceItem();
                else HandleDropItem(targetedSurface);
            }
            else
            {
                HandleGrabItem();
            }
        }
        else
        {
            if (isHoldingItem)
            {
                // Place or throw item depending on whether the raycast from camera hit anything
                HandleDropItem(targetedSurface);
            }
            else
            {
                HandleGrabItem();
            }
        }
    }

    private void HandleGrabItem()
    {
        var item = GrabItemRaycast();
        
        // A method for attacking would be here I guess
        if (item == null) return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Try storing item in inventory
            if(!_inventory.TryStoreItem(item))
                // TODO: Somehow let player know there's no place in their inventory. Possibly GrabItem()
                GrabItem(item.gameObject, item);
        }
        else
        {
            GrabItem(item.gameObject, item);
            SnapItemToCenter();
        }
    }

    private void HandleDropItem(Vector3 surface)
    {
        // TODO: Place or throw item depending on whether the raycast from camera hit anything
        DropItem();
    }
    
    private void HandleRightClick()
    {
        // TODO
        if(_isHoldingItem) RotateItem();
    }

    private bool IsItemOverInventory()
    {
        if (_heldItem == null || !_isInventoryOpen) return false;

        return _inventory.GetCellAtWorldPos(HoveredCellsMidPoint());
    }

    private Item GrabItemRaycast()
    {
        RaycastHit hit;
        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 10f, _itemMask))
        {
            var hitObject = hit.transform.gameObject;
            var item = hitObject.GetComponent<Item>();
            if (item != null) return item;
        }

        return null;
    }

    private void DropItem()
    {
        // TODO implement advanced logic
        _heldItem.transform.parent = null;
        _heldItem.GetComponent<Item>().OnDropped();
        _heldItem = null;
        _isHoldingItem = false;
    }

    private void RotateItem()
    {
        // TODO DEBUG
        _heldItem.GetComponent<Item>().Rotate(IsItemOverInventory() ? -1 * _inventory.GridParent.transform.forward : Vector3.zero);
    }

    private Vector3 HoveredCellsMidPoint()
    {
        var pos = Vector3.zero;
        foreach (var cell in _hoveredCells) 
        {
            pos += _inventory._grid.ObjectWorldPosition(cell);
        }

        pos /= _hoveredCells.Count;
        return pos;
    }

    // private void UpdatePointerPosition()
    // {
    //     if (!_holdingItem) return;
    //     PointerInputModule input = _eventSystem.currentInputModule as PointerInputModule;
    // }

    public void GrabItem(GameObject itemObj, Item item)
    {
        if (_isHoldingItem) return;
        itemObj.transform.parent = transform;
        // TODO Implement different offset for when hovering
        // itemObj.transform.rotation = _inventory.GridParent.transform.rotation;
        item.OnGrabbed();
        _heldItem = item.gameObject;
        _isHoldingItem = true;
        _inventory.RemoveItem(item);
        // TODO put it somewhere else
        item.OnInventoryExit();
    }
    
    public bool PlaceItem()
    {
        // var cells = HoveredCells();
        var item = _heldItem.GetComponent<Item>(); ;
        if (_hoveredCells.Count < item._inventoryWidth * item._inventoryHeight) return false;

        foreach (var cell in _hoveredCells)
        {
            if (!cell.IsEmpty())
                if (ShouldSwap())
                    return SwapItems();
                else return false;
        }
        
        var pos = Vector3.zero;
        _inventory.RemoveItem(item);
        
        // TODO probably make that an inventory method and call it from here

        if (!_inventory.TryStoreItem(item, _hoveredCells))
            throw new InvalidOperationException("Calling PlaceItem method returned false because item size is bigger than the hovered cells list");
        
        foreach (var cell in _hoveredCells) 
        {
            pos += _inventory._grid.ObjectWorldPosition(cell);
        }

        pos /= item._inventoryWidth * item._inventoryHeight;
        _heldItem.transform.position = pos + _inventory.GridParent.transform.forward * _inventory.ItemForwardOffset;
        _heldItem.transform.parent = null;
        _heldItem = null;
        _isHoldingItem = false;
        
        // TODO put it somewhere else
        item.OnInventoryEnter();
        return true;
    }

    private bool SwapItems()
    {
        var pos = Vector3.zero;
        var item = _heldItem.GetComponent<Item>();
        Item swapItem = null;
        
        foreach (var cell in _hoveredCells)
        {
            swapItem = cell.IsEmpty() ? swapItem : cell._storedItem;
            // probably unnecessary
            cell.DeleteItem();
        }
        
        _inventory.RemoveItem(swapItem);
        
        if (!_inventory.TryStoreItem(item, _hoveredCells))
            throw new InvalidOperationException("Calling SwapItem method returned false because item size is bigger than the hovered cells list");
        
        foreach (var cell in _hoveredCells)
        {
            pos += _inventory._grid.ObjectWorldPosition(cell);
        }
        
        // place held item
        pos /= item._inventoryWidth * item._inventoryHeight;
        _heldItem.transform.position = pos + _inventory.GridParent.transform.forward * _inventory.ItemForwardOffset;
        _heldItem.transform.parent = null;
        // TODO put it somewhere else
        item.OnInventoryEnter();
        
        // grab item swapped with
        var swapItemGameObj = swapItem.gameObject;
        swapItemGameObj.transform.parent = transform;
        // TODO Implement different offset for when hovering
        // swapItemGameObj.transform.rotation = _inventory.GridParent.transform.rotation;
        swapItem.OnGrabbed();
        _heldItem = swapItemGameObj;
        _isHoldingItem = true;
        // TODO put it somewhere else
        swapItem.OnInventoryExit();
        // HoveredCells();
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
                var offset = _inventory.CellSize * (x - (item._inventoryWidth - 1) * 0.5f) * _inventory.GridParent.right +
                             _inventory.CellSize * (y - (item._inventoryHeight - 1) * 0.5f) * _inventory.GridParent.up;
                
                var point = MousePositionInWorld() + offset;

                //debug
                points.Add(point);

                // var pointer = new PointerEventData(_eventSystem);
                // pointer.position = point;
                // pointer.hovered = new List<GameObject>();
                
                var cell = _inventory.GetCellAtWorldPos(point);
                if(cell != null)
                    hoveredCells.Add(cell);
                // cell?.OnPointerEnter(pointer);
            }
        }
        
        // if not all shot rays hit, offset them by two times the (average position of the rays that hit minus object center position)
        // if (IsCursorOverInventory() && hoveredCells.Count < item._inventoryWidth * item._inventoryHeight)
        if (hoveredCells.Count < item._inventoryWidth * item._inventoryHeight)
        {
            var snapOffset = Vector3.zero;
            foreach (var cell in hoveredCells)
            {
                snapOffset += _inventory._grid.ObjectWorldPosition(cell);
            }
            snapOffset /= hoveredCells.Count;
            snapOffset -= _heldItem.transform.position;
            snapOffset *= 2f;
            
            points.Clear();
            hoveredCells.Clear();


            for (int x = 0; x < item._inventoryWidth; x++)
            {
                for (int y = 0; y < item._inventoryHeight; y++)
                {
                    var offset = _inventory.CellSize * (x - (item._inventoryWidth - 1) * 0.5f) * _inventory.GridParent.right +
                                 _inventory.CellSize * (y - (item._inventoryHeight - 1) * 0.5f) * _inventory.GridParent.up;
                
                    var point = MousePositionInWorld() + offset + snapOffset;

                    //debug
                    points.Add(point);

                    var cell = _inventory.GetCellAtWorldPos(point);
                    if(cell != null)
                        hoveredCells.Add(cell);
                }
            }
        }

        var newCells = hoveredCells.Except(_hoveredCells);
        var removedCells = _hoveredCells.Except(hoveredCells);
        _hoveredCells = hoveredCells;
        
        // foreach (var cell in _hoveredCells)
        // {
        //     if (cell.IsEmpty())
        //         cell?.OnAvailable();
        //     else
        //     {
        //         if(ShouldSwap())
        //             cell?.OnCanSwap();
        //         else
        //             cell?.OnUnavailable();
        //     }
        // }

        bool unavailable = false;
        bool fullyHovered = _hoveredCells.Count == item._inventoryWidth * item._inventoryHeight;
        foreach (var cell in _hoveredCells) { unavailable |= !cell.IsEmpty(); }
        foreach (var cell in _hoveredCells)
        {
            if (!fullyHovered)
            {
                cell?.ResetCell();
                continue;
            }
            if(ShouldSwap())
                cell?.OnCanSwap();
            else
            if (unavailable)
                cell?.OnUnavailable();
            else
                cell?.OnAvailable();
        }
        
        foreach (var cell in removedCells)
        {
            cell?.OnPointerExit();
        }

        SnapToGrid();
    }

    private bool IsCursorOverInventory()
    {
        // // nullref exception when no item held
        // var point = MousePositionInWorld();
        // var cell = _inventory.GetCellAtWorldPos(point);
        // return cell != null;
        if (!_isInventoryOpen) return false;
        
        PointerEventData pointerEventData = new PointerEventData(_eventSystem);
        pointerEventData.position = Input.mousePosition;

        // Perform the raycast
        List<RaycastResult> results = new List<RaycastResult>(); // Adjust the array size based on your needs
        _raycaster.Raycast(pointerEventData, results);
        foreach (var result in results)
        {
            var cell = result.gameObject.GetComponentInParent<Inventory>();
            if (cell != null) return true;
        }

        // TODO: throw some exception maybe
        return false;
    }

    
    private Vector3 MousePositionInWorld()
    {
        // nullref exception when no item held
        if (!_isHoldingItem) throw new NullReferenceException("Called MousePositionInWorld with no object held");
        var distanceFromCamera = Vector3.Dot(_heldItem.transform.position - _mainCamera.transform.position, _mainCamera.transform.forward);
        var mousePositionInWorld = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceFromCamera));
        return mousePositionInWorld;
    }
    private Vector3 ScreenCenterPositionInWorld()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        var distanceFromCamera = Vector3.Dot(_heldItem.transform.position - _mainCamera.transform.position, _mainCamera.transform.forward);
        var screenCenterPositionInWorld = _mainCamera.ScreenToWorldPoint(new Vector3(screenCenter.x, screenCenter.y, distanceFromCamera));
        return screenCenterPositionInWorld;
    }

    private void SnapToGrid()
    {
        var item = _heldItem.GetComponent<Item>();
        
        if (_hoveredCells.Count < item._inventorySize) return;

        _heldItem.transform.rotation = Quaternion.Euler(_inventory.GridParent.transform.eulerAngles.x, _inventory.GridParent.transform.eulerAngles.y, item.LocalZAngle);
        _heldItem.transform.position = HoveredCellsMidPoint() + _inventory.GridParent.transform.forward * _inventory.ItemForwardOffset;
    }

    private bool ShouldSwap()
    {
        List<Item> hoveredObjects = new List<Item>();
        foreach (var cell in _hoveredCells)
        {
            if (!cell.IsEmpty() && !hoveredObjects.Contains(cell._storedItem)) hoveredObjects.Add(cell._storedItem);
        }

        return hoveredObjects.Distinct().ToList().Count == 1;
    }

    private void OnDrawGizmos()
    {
        if(!_isHoldingItem) return;
        
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
        _isInventoryOpen = true;
    }

    void UnlockCamera()
    {
        _isInventoryOpen = false;
    }

    private void SnapItemToCenter()
    {
        if (_isHoldingItem)
        {
            _heldItem.transform.position = ScreenCenterPositionInWorld();
            // Debug.Log(Input.mousePosition);
        }
    }

    private void OnDisable()
    {
        _inventory.OnOpenInventory -= OnOpenInventory;
        _inventory.OnCloseInventory -= OnCloseInventory;
    }

    public bool UICallbacks()
    {
        return _isInventoryOpen && !_isHoldingItem;
    }
    
    public bool CanPlaceItem()
    {
        return _isInventoryOpen && _isHoldingItem;
    }

    private void OnOpenInventory()
    {
        LockCamera();
    }

    private void OnCloseInventory()
    {
        UnlockCamera();
        SnapItemToCenter();
        _hoveredCells.Clear();
    }
}
