using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    Button startButton;
    TMPro.TMP_Dropdown polygonDropdown;

    // default selection in polygon menu is triangle
    int polygonSize = 3;

    // Start is called before the first frame update
    void Start()
    {
        startButton = GameObject.Find("Start Button").GetComponent<Button>();
        startButton.onClick.AddListener(StartButtonClicked);

        polygonDropdown = GameObject.Find("Polygon Dropdown").GetComponent<TMP_Dropdown>();
        polygonDropdown.onValueChanged.AddListener(PolygonDropdownChanged);
    }

    void PolygonDropdownChanged(int val)
    {
        // polygons are in the menu at position {sides} - 3
        polygonSize = val + 3;
    }

    void StartButtonClicked()
    {
        MainManager.Instance.SetPolygonSize(polygonSize);
        SceneManager.LoadScene("Game Scene");
    }
}
