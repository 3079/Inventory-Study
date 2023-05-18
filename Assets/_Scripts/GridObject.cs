using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GridObject<T>
{
    private int _width;
    private int _height;
    private float _cellSize;
    private Vector2 _anchor;
    private Transform _transform;
    private Vector3 _up;
    private Vector3 _right;
    private bool _debug;

    private T[,] _items;
    
    private int _gridCellCount => _width * _height;
    private float _gridWidth => _width * _cellSize;
    private float _gridHeight => _height * _cellSize;
    
    public GridObject (int height, int width, float cellSize, Transform transform,  Vector2 anchor, bool debug = false)
    {
        _height = height;
        _width = width;
        _cellSize = cellSize;
        _anchor = anchor;
        _transform = transform;
        _right = _transform.right;
        _up = _transform.up;
        _items = new T[width, height];
        _debug = debug;
        if (debug) OnDebug();
    }

    public void SetSize(int width, int height)
    {
        _width = width;
        _height = height;
        // TODO: resize items array and transfer items accordingly
        if (_debug) OnDebug();
    }

    public enum OffsetType {
        CENTER,
        BOTTOM_LEFT,
        BOTTOM_RIGHT,
        TOP_RIGHT,
        TOP_LEFT
    }

    private Vector3 BL => _transform.position
                          - _anchor.x * _gridWidth * _transform.right
                          - _anchor.y * _gridHeight * _transform.up;

    public Vector2 Offset(OffsetType offsetType = OffsetType.BOTTOM_LEFT)
    {
        Vector2 offset = offsetType switch
        {
            OffsetType.CENTER => (_up + _right) * 0.5f,
            OffsetType.BOTTOM_LEFT => Vector2.zero,
            OffsetType.BOTTOM_RIGHT => _right,
            OffsetType.TOP_RIGHT => _up + _right,
            OffsetType.TOP_LEFT => _up,
            _ => Vector2.zero
        };

        return offset;
    }

    public Vector3 PointWorldPosition(int x, int y, OffsetType offsetType = OffsetType.BOTTOM_LEFT)
    {
        var offset = Offset(offsetType);
        
        var BL = _transform.position
                 - _anchor.x * _gridWidth * _right
                 - _anchor.y * _gridHeight * _up;
        
        var point = BL + _right * _cellSize * (x + offset.x) + _up * _cellSize * (y + offset.y);
        return point;
    }

    public Vector3 ObjectWorldPosition(T obj, OffsetType offsetType = OffsetType.CENTER)
    {
        int x, y;
        GetObjectCoordinates(obj, out x, out y);
        return PointWorldPosition(x, y, offsetType);
    }

    public void GetXY(Vector3 worldPos, out int x, out int y)
    {
        // TODO add check whether Dots are positive (position is at the front facing side of the grid object)
        var localPos = worldPos - BL;
        var projectedX = Vector3.Dot(_transform.right, localPos);
        var projectedY = Vector3.Dot(_transform.up, localPos);
        x = Mathf.FloorToInt(projectedX / _cellSize);
        y = Mathf.FloorToInt(projectedY / _cellSize);
    }

    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < _width && y < _height;
    }

    public T GetObject(int x, int y)
    {
        if (!IsInBounds(x, y)) return default;
        
        return _items[x, y];
    }
    
    public T GetObject(Vector3 worldPos)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        return GetObject(x, y);
    }
    
    public void SetObject(int x, int y, T obj)
    {
        if (!IsInBounds(x, y)) return;
        
        _items[x, y] = obj;
    }
    
    public void SetObject(Vector3 worldPos, T obj)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        SetObject(x, y, obj);
    }

    public void GetObjectCoordinates(T obj, out int x, out int y)
    {
        x = -1;
        y = -1;
        
        for (int i = 0; i < _width; i++)
            for (int j = 0; j < _height; j++)
                if (_items[i, j].Equals(obj))
                {
                    x = i;
                    y = j;
                    return;
                }
    }

    private void OnDebug()
    {
        var BR = BL + _transform.right * _width * _cellSize;
        var TL = BL + _transform.up * _height * _cellSize;
        var TR = BR + TL - BL;
        // Debug.DrawLine(BL, BR);
        Debug.DrawLine(BR, TR, Color.white, 100f);
        Debug.DrawLine(TR, TL, Color.white, 100f);
        // Debug.DrawLine(TL, BL);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var point = PointWorldPosition(x, y);
                Debug.DrawLine(point, point + _transform.right * _cellSize, Color.white, 100f);
                Debug.DrawLine(point, point + _transform.up * _cellSize, Color.white, 100f);
            }
        }
    }

    public List<T> ToList()
    {
        List<T> list = new List<T>();
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
                list.Add(_items[x,y]);
        return list;
    }
}
