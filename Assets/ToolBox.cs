using UnityEngine;
using System;
using System.Collections.Generic;

public static class ToolBox
{
    private const float HorizontalOffsetFactor = 0.768f;
    private const float VerticalOffsetFactor = .475f;
    //compare two floats safely
    public static bool ApproximatelyEqual(float a, float b, float epsilon)
    {
        return Mathf.Abs(a - b) < epsilon;
    }
    
    //NormalRandom.cs
    
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
    
    
    public class NormalRandom
    {
        private readonly double _standardDeviation;
        private readonly double _mean;

        private readonly System.Random _random;

        public NormalRandom(double standardDeviation, double mean)
        {
            _standardDeviation = standardDeviation;
            _mean = mean;
            _random = new System.Random();
        }

        // Generate a random number with normal distribution
        public int RandomNum(double lowerBound, double upperBound)
        {
            double num = NextGaussian() * _standardDeviation + _mean;

            if (num < lowerBound)
            {
                num = lowerBound;
            }
            else if (num > upperBound)
            {
                num = upperBound;
            }

            return (int)num;
        }

        // Generate a normally distributed random number
        private double NextGaussian()
        {
            double u1 = 1.0 - _random.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - _random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                   Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return randStdNormal;
        }
    }
    
    
}
