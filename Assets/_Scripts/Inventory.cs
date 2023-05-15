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
    public float ItemForwardOffset => _itemForwardOffset;
    [SerializeField] private bool _debug;
    [SerializeField] private GameObject _gridCell;
     
    public GridObject<InventoryCell> _grid { get; private set; }
    // private List<InventoryCell> _cells;

    public event Action OnOpenInventory;
    public event Action OnCloseInventory;

    private void Awake()
    {
        _grid = new GridObject<InventoryCell>(_height, _width, _cellSize, _gridParent, _anchor, _debug);
        
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
}
