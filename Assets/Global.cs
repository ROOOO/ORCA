﻿using UnityEngine;
using System.Collections;

public class Global {
    public static int maxTimes = 10000;   // max update position times

    public static float t = 6.0f;
    public static int prefTimes = 500;    // prefer to finish in update times as least prefTime
    public static float stepTime = 0.01f; // assume the delta time per update
    public static float maxNeighborRange = 5.0f;

    public static Vector2 rotate(Vector2 v, float cos, float sin)
    {
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }

    public static float det(Vector2 v1, Vector2 v2)
    {
        return v1.x * v2.y - v2.x * v1.y;
    }
}

public class Line
{
    public Vector2 direction;
    public Vector2 point;
}
