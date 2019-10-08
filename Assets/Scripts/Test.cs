using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    void Update()
    {
        Debug.Log($"{Mouse.current.scroll.x.ReadValue()} {Mouse.current.scroll.y.ReadValue()}");
    }
}
