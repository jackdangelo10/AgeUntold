using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexIterator
{
    private Vector2 _currentPosition;
    private const float HorizontalOffsetFactor = 0.768f;
    private const float VerticalOffsetFactor = .475f;
    
    
    public enum Axis
    {
        southwestToNortheast,
        eastToWest,
        southeastToNorthwest,
        northwestToSoutheast,
        westToEast,
        northeastToSouthwest
    }

    private readonly Axis _iterationAxis;

    public HexIterator(Vector2 startPosition, Axis axis)
    {
        this._currentPosition = startPosition;
        this._iterationAxis = axis;
    }

    public HexType Next()
    {
        switch (_iterationAxis)
        {
            case Axis.southwestToNortheast:
                return NextNortheast();
            case Axis.eastToWest:
                return NextWest();
            case Axis.southeastToNorthwest:
                return NextNorthwest();
            case Axis.northwestToSoutheast:
                return NextSoutheast();
            case Axis.westToEast:
                return NextEast();
            case Axis.northeastToSouthwest:
                return NextSoutheast();
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }

    public HexType Previous()
    {
        switch (_iterationAxis)
        {
            case Axis.southwestToNortheast:
                return NextSouthwest();
            case Axis.eastToWest:
                return NextEast();
            case Axis.southeastToNorthwest:
                return NextSoutheast();
            case Axis.northwestToSoutheast:
                return NextNortheast();
            case Axis.westToEast:
                return NextWest();
            case Axis.northeastToSouthwest:
                return NextNortheast();
            default:
                throw new System.ArgumentOutOfRangeException();
        }
    }
    
    
    public HexType NextNortheast()
    {
        _currentPosition = new Vector2(_currentPosition.x + HorizontalOffsetFactor / 2, _currentPosition.y + VerticalOffsetFactor);
        return GetCollision();
    }
    
    public HexType NextEast()
    {
        _currentPosition = new Vector2(_currentPosition.x + HorizontalOffsetFactor, _currentPosition.y);
        return GetCollision();
    }
    
    public HexType NextSoutheast()
    {
        _currentPosition = new Vector2(_currentPosition.x + HorizontalOffsetFactor / 2, _currentPosition.y - VerticalOffsetFactor);
        return GetCollision();
    }
    
    public HexType NextSouthwest()
    {
        _currentPosition = new Vector2(_currentPosition.x - HorizontalOffsetFactor / 2, _currentPosition.y - VerticalOffsetFactor);
        return GetCollision();
    }
    
    public HexType NextWest()
    {
        _currentPosition = new Vector2(_currentPosition.x - HorizontalOffsetFactor, _currentPosition.y);
        return GetCollision();
    }
    
    public HexType NextNorthwest()
    {
        _currentPosition = new Vector2(_currentPosition.x - HorizontalOffsetFactor / 2, _currentPosition.y + VerticalOffsetFactor);
        return GetCollision();
    }

    private HexType GetCollision()
    {
        RaycastHit2D hit = Physics2D.Raycast(_currentPosition, _currentPosition, 0, LayerMask.GetMask("Default"));
        if (hit)
        {
            HexType newHex = hit.collider.gameObject.GetComponent<HexType>();
            return newHex;
        }
        else
        {
            return null;
        }
    }
    
    //nab some neighbors, ordered clockwise from 0 to 5, starting in northeast
    public static List<Vector2> GetNeighbors(Vector2 currentPosition)
    {
        List<Vector2> neighbors = new List<Vector2>();
        
        neighbors.Add(new Vector2(currentPosition.x + HorizontalOffsetFactor / 2, currentPosition.y + VerticalOffsetFactor)); //0
        neighbors.Add(new Vector2(currentPosition.x + HorizontalOffsetFactor, currentPosition.y));                              //1
        neighbors.Add(new Vector2(currentPosition.x + HorizontalOffsetFactor / 2, currentPosition.y - VerticalOffsetFactor)); //2
        neighbors.Add(new Vector2(currentPosition.x - HorizontalOffsetFactor / 2, currentPosition.y - VerticalOffsetFactor)); //3
        neighbors.Add(new Vector2(currentPosition.x - HorizontalOffsetFactor, currentPosition.y));                              //4
        neighbors.Add(new Vector2(currentPosition.x - HorizontalOffsetFactor / 2, currentPosition.y + VerticalOffsetFactor)); //5
        
        return neighbors;
    }
}