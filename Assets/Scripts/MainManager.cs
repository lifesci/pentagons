using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;
    public int polygonSize { get; private set; }

    private void Awake()
    {
        if (Instance is not null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPolygonSize(int size)
    {
        polygonSize = size;
    }
}
