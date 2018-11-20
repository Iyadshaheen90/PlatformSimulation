using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerExample2 : MonoBehaviour {

    // RectTransform = transform for the panel (?)
    public RectTransform panelPlatformConfig;
    bool displayConfigPanelBool = false;

    private void Start()
    {
        panelPlatformConfig.gameObject.SetActive(displayConfigPanelBool);
    }

    public void DisplayConfigPanels()
    {
        displayConfigPanelBool = !displayConfigPanelBool;
        panelPlatformConfig.gameObject.SetActive(displayConfigPanelBool);
    }


}
