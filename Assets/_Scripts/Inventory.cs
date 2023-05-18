using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// public class Inventory : MonoBehaviour, IInteractable
// {
//     [SerializeReference] private List<GameObject> _items = new List<GameObject>();
//     [SerializeField] private GameObject _menu;
//     [SerializeField] private GameObject _panelPrefab;
//     [SerializeField] private GameObject _grid;
//     
//     [Space]
//     [Header("Settings")]
//     [SerializeField] private int _width;
//     [SerializeField] private int _height;
//     [SerializeField] private float _cellSize;
//
//     private int _inventorySize => _width * _height;
//
//     public event Action OnOpenInventory;
//     public event Action OnCloseInventory;
//     void Start()
//     {
//         // set up grid according to settings
//         // spawn panels
//         SetUpGrid();
//         CloseInventory();
//     }
//
//     void SetUpGrid()
//     {
//         var menu = _menu.GetComponent<RectTransform>();
//         // _cellSize = Mathf.Min(menu.rect.size.x / _width, menu.rect.y / _height);
//         // _cellSize = menu.rect.size.x / _width;
//         
//         // TODO: dynamically determine size of the cell from width and height
//         
//         menu.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _width * _cellSize);
//         menu.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _height * _cellSize);
//         var gridLayoitGriup = _grid.GetComponent<GridLayoutGroup>();
//         gridLayoitGriup.cellSize = Vector2.one * _cellSize;
//         gridLayoitGriup.constraintCount = _width;
//         for (int i = 0; i < _inventorySize; i++)
//         {
//             var panel = Instantiate(_panelPrefab, _grid.transform.position, _grid.transform.rotation, _grid.transform);
//         }
//     }
//
//     public void Interact()
//     {
//         if (_menu.activeSelf) CloseInventory();
//         else OpenInventory();
//     }
//
//     void OpenInventory()
//     {
//         _menu.SetActive(true);
//         Cursor.visible = true;
//         Cursor.lockState = CursorLockMode.Confined;
//         OnOpenInventory?.Invoke();
//     }
//     
//     void CloseInventory()
//     {
//         _menu.SetActive(false);
//         Cursor.visible = false;
//         Cursor.lockState = CursorLockMode.Locked;
//         OnCloseInventory?.Invoke();
//     }
// }


public class Inventory : MonoBehaviour, IInteractable
{
    [SerializeReference] private List<GameObject> _items = new List<GameObject>();
    [SerializeField] private GameObject _menu;
    [SerializeField] private GameObject _panelPrefab;

    [Space]
    [Header("Settings")]
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private float _cellSize;
    public float CellSize => _cellSize;
    [SerializeField] private Vector2 _anchor;
    [SerializeField] private Transform _gridParent;
    public Transform GridParent => _gridParent;
    [SerializeField] private float _itemForwardOffset = -0.1f;
    [SerializeField] private float _itemForwardOffsetHovering = -0.2f;
    public float ItemForwardOffset => _itemForwardOffset;
    public float ItemForwardOffsetHovering => _itemForwardOffsetHovering;
    [SerializeField] private bool _debug;
    [SerializeField] private GameObject _gridCell;
     
    public GridObject<InventoryCell> _grid { get; private set; }
    // private List<InventoryCell> _cells;
    private CameraController _cameraController;

    public event Action OnOpenInventory;
    public event Action OnCloseInventory;

    private void Awake()
    {
        _grid = new GridObject<InventoryCell>(_height, _width, _cellSize, _gridParent, _anchor, _debug);
        _cameraController = FindObjectOfType<CameraController>();
    }

    void Start()
    {
        // set up grid according to settings
        SetUpGrid();
        CloseInventory();
    }

    void SetUpGrid()
    {
        var menu = _menu.GetComponent<RectTransform>();
        // _cellSize = Mathf.Min(menu.rect.size.x / _width, menu.rect.y / _height);
        // _cellSize = menu.rect.size.x / _width;
             
        // TODO: dynamically determine size of the cell from width and height
             
        menu.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _width * _cellSize);
        menu.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _height * _cellSize);
        var gridLayoutGroup = _menu.GetComponentInChildren<GridLayoutGroup>();
        gridLayoutGroup.cellSize = Vector2.one * _cellSize;
        gridLayoutGroup.constraintCount = _width;
        // _cells = new List<InventoryCell>();
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                // var cell = Instantiate(_gridCell, _grid.PointWorldPosition(x, y, GridObject<InventoryCell>.OffsetType.CENTER), _gridParent.rotation, _gridParent);
                var cell = Instantiate(_panelPrefab, gridLayoutGroup.transform.position, gridLayoutGroup.transform.rotation, gridLayoutGroup.transform);
                var inventoryCell = cell.GetComponent<InventoryCell>();
                _grid.SetObject(x, y, inventoryCell);
                // _cells.Add(inventoryCell);
            }
        }
    }

    public void Interact()
    {
        if (_menu.activeSelf) CloseInventory();
        else OpenInventory();
    }

    private void OpenInventory()
    {
        _menu.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        OnOpenInventory?.Invoke();
    }
     
    private void CloseInventory()
    {
        _menu.SetActive(false);
        
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                _grid.GetObject(x, y).ResetCell();
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        OnCloseInventory?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.grey;
        var BL = _gridParent.position
                 - _anchor.x * _width * _cellSize * _gridParent.right
                 - _anchor.y * _height * _cellSize * _gridParent.up;
        var BR = BL + _gridParent.right * _width * _cellSize;
        var TL = BL + _gridParent.up * _height * _cellSize;
        var TR = BR + TL - BL;
        Gizmos.DrawLine(BL, BR);
        Gizmos.DrawLine(BR, TR);
        Gizmos.DrawLine(TR, TL);
        Gizmos.DrawLine(TL, BL);
    }

    public InventoryCell GetCellAtWorldPos(Vector3 worldPos)
    {
        // int x, y;
        // _grid.GetXY(worldPos, out x, out y);
        // Debug.Log("Hit cell at coordinates (" + x +", " + y + ")");
        return _grid.GetObject(worldPos);
        // return _grid.GetObject(x, y);
    }

    public void SetItemColor(Item item, InventoryCell.ColorType color)
    {
        foreach (var cell in _grid.ToList())
        {
            if (cell._storedItem == item)
                cell.SetColor(color);
        }
    }

    public void ResetItemCells(Item item)
    {
        foreach (var cell in _grid.ToList())
        {
            if (cell._storedItem == item)
                cell.ResetCell();
        }
    }
    
    public void RemoveItem (Item item)
    {
        foreach (var cell in _grid.ToList())
        {
            if (cell._storedItem == item)
                cell.DeleteItem();
        }
    }

    public List<InventoryCell> GetItemCells(Item item)
    {
        List<InventoryCell> list = new List<InventoryCell>();
        foreach (var cell in _grid.ToList())
        {
            if (cell._storedItem == item)
                list.Add(cell);
        }

        return list;
    }

    public bool TryStoreItem(Item item)
    {
        // TODO
        Debug.Log("TryStoreItem method called");
        List<InventoryCell> freeCells = new List<InventoryCell>();
        
        // searching for free space in base orientation
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                freeCells = SearchForFreeCells(x, y, item._inventoryWidth, item._inventoryHeight);

                if (freeCells.Count < item._inventorySize)
                {
                    freeCells.Clear();
                    continue;
                }

                Debug.Log("Found free space in base orientation");
                return StoreItem(item, freeCells);
            }
        }
        
        // searching for free space in rotated orientation
        Debug.Log("Found no free space in base orientation, rotating object and trying again");
        freeCells.Clear();
            
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                freeCells = SearchForFreeCells(x, y, item._inventoryHeight, item._inventoryWidth);

                if (freeCells.Count < item._inventorySize)
                {
                    freeCells.Clear();
                    continue;
                }
                    
                Debug.Log("Found free space rotated");
                item.Rotate(Vector3.zero);
                Debug.Log(item.transform.eulerAngles.z);
                return StoreItem(item, freeCells);
            }
        }
        
        Debug.Log("Found no free space");
        return false;
    }

    private bool StoreItem(Item item, List<InventoryCell> cells)
    {
        if (TryStoreItem(item, cells))
        {
            var pos = Vector3.zero;
            foreach (var c in cells) 
            {
                pos += _grid.ObjectWorldPosition(c);
            }
            pos /= item._inventorySize;
            item.transform.position = pos + _gridParent.transform.forward * _itemForwardOffset;
            Debug.Log(item.transform.eulerAngles.z);
            item.transform.rotation = Quaternion.Euler(_gridParent.transform.eulerAngles.x, _gridParent.transform.eulerAngles.y, item.LocalZAngle);
            // TODO put it somewhere else
            item.OnInventoryEnter();
            Debug.Log("Successfully stored item");
            return true;
        }
                
        Debug.Log("Storing item failed");
        return false;
    }

    private List<InventoryCell> SearchForFreeCells(int x, int y, int width, int height)
    {
        List<InventoryCell> list = new List<InventoryCell>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var cell = _grid.GetObject(x + i, y + j);
                if (cell == null)
                {
                    Debug.Log("Cell " + (x + i) + ", " + (y + j) + " is null");
                    return list;
                }
                if (!cell.IsEmpty()) return list;
                list.Add(cell);
            }
        }

        return list;
    }
    
    public bool TryStoreItem(Item item, List<InventoryCell> cells)
    {
        if (cells.Count < item._inventorySize)
            return false;
        
        foreach (var cell in cells) 
        {
            cell.StoreItem(item);
        }

        return true;
    }
}
