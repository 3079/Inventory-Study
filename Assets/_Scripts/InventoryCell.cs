using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class InventoryCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IDropHandler
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
    // [SerializeField] public Item _storedItem { get; private set; }
    public Item _storedItem;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _inventory = GetComponentInParent<Inventory>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_storedItem == null)
            _image.color = _hoverColor;
        else
        {
            // TODO:
            // get size of the stored object and highlight all the cells occupied by it
            // or maybe just scan all the cells and highlight ones that store the same object
            _inventory.SetItemColor(_storedItem, ColorType.HOVER);
            var x = _storedItem._inventoryWidth;
            var y = _storedItem._inventoryHeight;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_storedItem == null)
            ResetCell();
        else
        {
            _inventory.SetItemColor(_storedItem, ColorType.HOLDING_ITEM);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _image.color = _holdingItemColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _image.color = eventData.hovered.Contains(gameObject) ? _hoverColor : _baseColor;
    }

    public void OnDrag(PointerEventData eventData)
    {
        ResetCell();
    }

    public void OnDrop(PointerEventData eventData)
    {
        ResetCell();
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
        _image.color = _baseColor;
    }
}
