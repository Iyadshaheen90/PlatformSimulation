using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerExample : MonoBehaviour {

    bool simulating = false;

    public Text simStatusDisplay;
    public Slider mySlider;
    public InputField myInputField;
    public Dropdown myDropdown;
    public Text dropdownText;

    bool displayConfigPanel = false;
    public RectTransform panelConfig;

	// Use this for initialization
	void Start () {
        simStatusDisplay.text = "sleeping...";
        dropdownText.text = myDropdown.ToString();

        panelConfig.gameObject.SetActive(displayConfigPanel);
    }
	
	// Update is called once per frame
	void Update () {

        /*
        if (simulating)
            Debug.Log("We are fucking simulating this!");
        else
            Debug.Log("We are not fucking");
        */

    }

    public void ButtonClicked()
    {
        simulating = !simulating;
        //simStatusDisplay.text = simulating.ToString();

        if (simulating)
            simStatusDisplay.text = "FUCKING";
        else
            simStatusDisplay.text = "sleeping...";
    }

    public void SliderValueChange(Slider s)
    {
        //myInputField.text = "" + s.value;
        myInputField.text = s.value.ToString();
    }

    public void DropdownValueText(Dropdown d)
    {
        dropdownText.text = d.value.ToString();
    }

    public void ConfigPopUp()
    {
        displayConfigPanel = true;
    }
}
