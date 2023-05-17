using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class InventoryCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IDropHandler, IPointerClickHandler
// public class InventoryCell : MonoBehaviour
{
    [SerializeField] private Sprite _cellBackground;
    [SerializeField] private TextMeshProUGUI _stacksText;

    public enum ColorType
    {
        BASE,
        HOVER,
        HOLDING_ITEM,
        AVAILABLE,
        SWAP,
        UNAVAILABLE,
        SELECTED
    }
    
    [Header("Color Settings")]
    [SerializeField] private Color _baseColor;
    [SerializeField] private Color _hoverColor;
    [SerializeField] private Color _holdingItemColor;
    [SerializeField] private Color _availableColor;
    [SerializeField] private Color _swapColor;
    [SerializeField] private Color _unavailableColor;
    [SerializeField] private Color _selectedColor;
    
    private Image _image;
    private Inventory _inventory;

    private CameraController _cameraController;
    // [SerializeField] public Item _storedItem { get; private set; }
    public Item _storedItem;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _inventory = GetComponentInParent<Inventory>();
        _cameraController = FindObjectOfType<CameraController>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_cameraController.UICallbacks()) return;
        if (IsEmpty())
            _image.color = _hoverColor;
        else
        {
            // TODO:
            // get size of the stored object and highlight all the cells occupied by it
            // or maybe just scan all the cells and highlight ones that store the same object
            foreach (var cell in _inventory.GetItemCells(_storedItem))
                _inventory.SetItemColor(_storedItem, ColorType.HOVER);
            var x = _storedItem._inventoryWidth;
            var y = _storedItem._inventoryHeight;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_cameraController.UICallbacks()) return;
        ResetCell();
    }
    public void OnPointerExit()
    {
        ResetCell();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_cameraController.UICallbacks() || _storedItem != null) return;
        _image.color = _holdingItemColor;
        
        // TODO call cameraController grab method on stored item? or place or swap
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_cameraController.UICallbacks() || _storedItem != null) return;
        if (IsEmpty()) ResetCell();
        else
        {
            if (eventData.hovered.Contains(_storedItem.gameObject))
                _inventory.SetItemColor(_storedItem, ColorType.HOVER);
        }
        OnPointerEnter(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_cameraController.UICallbacks()) return;
        ResetCell();
    }

    public void ClearCell()
    {
        _image.color = _baseColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!_cameraController.UICallbacks()) return;
        ResetCell();
    }
    
    public void OnAvailable()
    {
        _image.color = _availableColor;
    }

    public void OnUnavailable()
    {
        _image.color = _unavailableColor;
    }
    
    public void OnCanSwap()
    {
        _image.color = _swapColor;
    }

    public void SetColor(ColorType colorType)
    {
        _image.color = colorType switch
        {
            ColorType.BASE => _baseColor,
            ColorType.HOVER => _hoverColor,
            ColorType.SELECTED => _selectedColor,
            ColorType.HOLDING_ITEM => _holdingItemColor,
            ColorType.AVAILABLE => _availableColor,
            ColorType.SWAP => _swapColor,
            ColorType.UNAVAILABLE => _unavailableColor,
            _ => _baseColor
        };
    }

    public void ResetCell()
    {
        if (IsEmpty())
            ClearCell();
        else
        {
            _inventory.SetItemColor(_storedItem, ColorType.HOLDING_ITEM);
        }
    }

    public void Debug()
    {
        _image.color = Color.green;
    }

    public void StoreItem(Item item)
    {
        _storedItem = item;
        _image.color = _holdingItemColor;
    }
    
    public void DeleteItem()
    {
        _storedItem = null;
        ClearCell();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_cameraController.UICallbacks())
        {
            if (_cameraController.CanPlaceItem())
                _cameraController.PlaceItem();
            else return;
        }
        else
        {
            if (_storedItem != null)
                _cameraController.GrabItem(_storedItem.gameObject, _storedItem);
        }
    }

    public Item GrabItem()
    {
        if (IsEmpty()) return null;
        
        var item = _storedItem;
        _storedItem = null;
        _image.color = _availableColor;
        ClearCell();
        return item;
    }

    public bool IsEmpty()
    {
        return _storedItem == null;
    }
}
