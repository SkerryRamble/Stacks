using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticEnums
{
    public enum GameEndStatus { Won, Lost, Paused, OnGoing };

    public enum BuildMode { Building, Selling, Upgrading, Nothing };

    public enum TargettingPriority { First, Last, Nearest, Strongest, Weakest};

    public enum GameSpeed
    {   
        //We need the suffix Speed to avoid naming conflicts with Double
        //The values assigned are 10 times the actual rate, due to not being able to use floats for enum values, affecting the halfSpeed calc
        //For example, we use deltaTime * halfSpeed/10f
        HalfSpeed = 5, 
        NormalSpeed = 10, 
        DoubleSpeed = 20, 
        TripleSpeed = 30
    }
}