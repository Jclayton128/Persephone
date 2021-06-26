using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MouseHelper : object
{
    public static Vector2 GetMouseCursorLocation()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 flatPos = worldPosition;
        return flatPos;
    }
}
