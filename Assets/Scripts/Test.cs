using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    void Start()
    {
        List<int> test = new List<int>();

        for (int i = 0; i < 63; i++)
        {
            test.Add(i);
        }
        test.Clear();
        Debug.Log(test.Capacity);
    }
}
