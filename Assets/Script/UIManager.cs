using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    // ----- variables -----
    
    public PlatformManager platformSimManager; // holds PlatformSim script

    public Dropdown colorDropdown;
    public Slider mSlider;
    public Slider nSlider;
    public Slider spacingSlider;
    public Slider yMaxSlider; // maximum y-axis (PlatformConfig scene)

    public Slider heightSlider; // individual node height (Programming scene)

    // holds all the value for initializing the platform
    int mValue = 16;
    int nValue = 9;
    float spacingValue = 0.1f;
    float heightValue = 1.0f;

    // RectTransform = transform for the panel
    public RectTransform panelSetup;
    
    //bool displaySetupPanelBool = true;


    // ----- delegates and events -----

    public delegate void BuildPlatformClicked(PlatformConfigurationData pcd);
    public static event BuildPlatformClicked BuildPlatformOnClicked;

    public delegate void WriteProgramData(PlatformConfigurationData pcd);
    public static event WriteProgramData OnWriteProgramData;

    //public delegate void UpdateCameraPosition(PlatformConfigurationData pcd);
    //public static event UpdateCameraPosition OnUpdateCameraPosition;

    public delegate void NodeProgramChanged(float val);
    public static event NodeProgramChanged OnNodeProgramChanged;

    // subscribing delegates and events
    private void OnEnable()
    {
        PlatformManager.OnPlatformManagerChanged += PlatformManager_OnPlatformManagerChanged;
        //PlatformManager.OnPlatformManagerUpdateUI += PlatformManager_OnPlatformManagerUpdateUI;

        PlatformDataNode.OnUpdatePlatformDataNodeUI += PlatformDataNode_OnUpdatePlatformDataNodeUI;
    }

    // unsubscribing delegates and events
    private void OnDisable()
    {
        PlatformManager.OnPlatformManagerChanged -= PlatformManager_OnPlatformManagerChanged;
        //PlatformManager.OnPlatformManagerUpdateUI -= PlatformManager_OnPlatformManagerUpdateUI;

        PlatformDataNode.OnUpdatePlatformDataNodeUI -= PlatformDataNode_OnUpdatePlatformDataNodeUI;
    }

    // updating Programming scene UI using data from node
    private void PlatformDataNode_OnUpdatePlatformDataNodeUI(PlatformDataNode pdn)
    {
        GameObject.Find("TextNodeName").GetComponent<Text>().text = string.Format("Node[{0},{1}]", pdn.iPosition, pdn.jPosition);
        GameObject.Find("TextHeightNum").GetComponent<Text>().text = "" + pdn.yPosition;

        heightSlider.maxValue = PlatformManager.Instance.configData.height; // set maxValue to the slider
        heightSlider.value = pdn.yPosition;
    }

    private void PlatformManager_OnPlatformManagerChanged(PlatformConfigurationData pcd)
    {
        if (pcd != null)
        {
            if (SceneManager.GetActiveScene().name.Equals("PlatformConfig"))
            {
                // ----- updating config panel UI and correcting the variables' value -----

                Debug.Log("pcd != null, updating PlatformConfig UI to reflect current data");

                mSlider.value = pcd.mSize;
                MSliderValue(mSlider);

                nSlider.value = pcd.nSize;
                NSliderValue(nSlider);

                spacingSlider.value = pcd.deltaSpace;
                SpacingSliderValue(spacingSlider);

                heightValue = pcd.height;
                YMaxSliderValue(yMaxSlider);
                //GameObject.Find("TextYRangeNum").GetComponent<Text>().text = "" + heightValue; // update text

                // ----- top panel update -----
                TopPanelUpdate();

                // ----- update camera -----

            }
            if (SceneManager.GetActiveScene().name.Equals("Programming") || 
                SceneManager.GetActiveScene().name.Equals("Simulate"))
            {
                // updating UI for size and spacing (top panel)
                GameObject.Find("TextMxN").GetComponent<Text>().text = pcd.mSize + " x " + pcd.nSize;
                GameObject.Find("TextSpacingFloat").GetComponent<Text>().text = "" + pcd.deltaSpace;
            }
        }
    }

    //private void PlatformManager_OnPlatformManagerUpdateUI(string nodeName)
    //{
    //    // UI interaction in programming goes here to PlatformManager
    //    //GameObject.Find("TextNodeName").GetComponent<Text>().text = nodeName;

    //    //heightSlider.value = 0f;

    //}

    void Start()
    {
        //if (SceneManager.GetActiveScene().name == "PlatformConfig")
        //{
        //    Debug.Log("UITextInitiaize from Start");
        //    UITextInitialize();
        //}

    }

    // all UI update happening in real time is made in here
    // TODO: remove all these from Update() and use delegates for each of them
    //void Update()
    //{
    //    // updating y-axis range (top panel) in real time
    //    GameObject.Find("TextYAxisNum").GetComponent<Text>().text = "" + platformSimManager.GetDisplacementRange();

    //    // updating y-axis range (if setup panel is displayed) in real time
    //    if (displaySetupPanelBool) 
    //        GameObject.Find("TextYRangeNum").GetComponent<Text>().text = "" + platformSimManager.GetDisplacementRange();

    //    // updating color panel in real time
    //    GameObject.Find("DropdownColor").GetComponent<Dropdown>().value = platformSimManager.GetColorOption();

    //    // updating Start/Stop button (will also update itself when user pressed 'T' key)
    //    if (platformSimManager.GetSimStatus() == true)
    //        GameObject.Find("ButtonStart").GetComponentInChildren<Text>().text = "STOP";
    //    else
    //        GameObject.Find("ButtonStart").GetComponentInChildren<Text>().text = "START";
    //}

    // handling all button behavior here
    public void ButtonClick(Button b)
    {
        switch (b.name)
        {
            // ---------- jumping scenes ----------
            case "btnMainMenu":
                Debug.Log("Jumping to MainMenu scene..");
                SceneManager.LoadScene("MainMenu");
                break;
            case "btnSetup":
                Debug.Log("Jumping to PlatformConfig scene..");
                SceneManager.LoadScene("PlatformConfig");
                break;
            case "btnProgram":
                Debug.Log("Jumping to Programming scene..");
                SceneManager.LoadScene("Programming");

                //if (PlatformManager.Instance.configData != null)
                //{
                //    OnUpdateCameraPosition(PlatformManager.Instance.configData);
                //    //BuildPlatformOnClicked(PlatformManager.Instance.configData);
                //}
                break;
            case "btnSimulate":
                // check if we got a save file
                if (System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "WriteLines.txt")))
                {
                    Debug.Log("Save file found!");
                    Debug.Log("Jumping to Simulate scene..");
                    SceneManager.LoadScene("Simulate");
                }
                else
                {
                    Debug.Log("No save file found!");
                }
                break;
            case "btnExit":
                Debug.Log("Exiting program...");
                Application.Quit();
                break;

            // ---------- setup scene ----------
            case "ButtonBuild":
                // building the platform using set values from UI first..
                if (BuildPlatformOnClicked != null)
                {
                    Debug.Log("Green button clicked! Building platform...");

                    // initialize PCD
                    PlatformConfigurationData pcd;
                    pcd = gameObject.AddComponent<PlatformConfigurationData>();

                    // pushing all the values set on UI to PCD
                    pcd.mSize = mValue;
                    pcd.nSize = nValue;
                    pcd.deltaSpace = spacingValue;
                    pcd.height = heightValue;
                    pcd.color = colorDropdown.value;

                    // send PCD data to consumer (PlatformManager & Camera)
                    BuildPlatformOnClicked(pcd);

                    TopPanelUpdate(); // update top panel UI
                }
                break;

            // ---------- programming scene ----------
            case "ButtonProgram":
                // create save file 
                if (OnWriteProgramData != null)
                {
                    Debug.Log("platform programming complete. writing save file");
                    OnWriteProgramData(PlatformManager.Instance.configData);
                }
                break;

            // ---------- default case ----------
            default:
                break;
        }
    }

    // initializing UI text
    private void UITextInitialize()
    {
        // initializing top panel
        GameObject.Find("TextMxN").GetComponent<Text>().text = mValue + " x " + nValue;
        GameObject.Find("TextSpacingFloat").GetComponent<Text>().text = "" + spacingValue;
        GameObject.Find("TextYAxisNum").GetComponent<Text>().text = "" + heightValue;

        // initializing setup panel
        //panelSetup.gameObject.SetActive(displaySetupPanelBool); // hide setup panel
        GameObject.Find("TextYRangeNum").GetComponent<Text>().text = "" + heightValue;

        // initializing sliders' value
        mSlider.value = mValue;
        nSlider.value = nValue;
        spacingSlider.value = spacingValue;
    }

    // Start/Stop button on the top panel
    public void ButtonStart()
    {
        platformSimManager.SetSimulation();
    }

    // Exit button on top panel
    public void ButtonExit()
    {
        platformSimManager.QuitApp();
    }

    // Setup button on top panel
    //public void DisplaySetupPanel()
    //{
    //    displaySetupPanelBool = !displaySetupPanelBool;
    //    panelSetup.gameObject.SetActive(displaySetupPanelBool);
    //}

    // + button on Setup panel
    //public void YRangePlus()
    //{
    //    //platformSimManager.RangePlus();

    //    heightValue += 0.1f;
    //    heightValue = Mathf.Round(heightValue * 10f) / 10f; // set value to just 1 decimal point
    //    if (heightValue > 1f) heightValue = 1f; // set upper limit to 1
    //    //Debug.Log("new heightValue = " + heightValue);

    //    GameObject.Find("TextYRangeNum").GetComponent<Text>().text = "" + heightValue; // update text
    //}

    // - button on Setup panel
    //public void YRangeMinus()
    //{
    //    //platformSimManager.RangeMinus();

    //    heightValue -= 0.1f;
    //    heightValue = Mathf.Round(heightValue * 10f) / 10f; // set value to just 1 decimal point
    //    if (heightValue < 0f) heightValue = 0f; // set lower limit to 0
    //    //Debug.Log("new heightValue = " + heightValue);

    //    GameObject.Find("TextYRangeNum").GetComponent<Text>().text = "" + heightValue; // update text
    //}

    // Dropdown Color menu
    public void ColorChange()
    {
        int currentvalue = colorDropdown.value;
        //Debug.Log("Color Dropdown value: " + currentvalue);

        switch (currentvalue)
        {
            case 0: // grayscale on dropdown
                platformSimManager.SetColorOption(0); 
                break;
            case 1: // red
                platformSimManager.SetColorOption(1);
                break;
            case 2: // green
                platformSimManager.SetColorOption(2);
                break;
            case 3: // blue
                platformSimManager.SetColorOption(3);
                break;
            //case 4: // RGB
            //    platformSimManager.SetColorOption(4);
            //    break;
            default:
                break;
        }
    }

    // Slider to set size m
    public void MSliderValue(Slider ms)
    {
        mValue = (int)ms.value;
        //Debug.Log("M Slider Value: " + mValue);
        GameObject.Find("TextMValueNum").GetComponent<Text>().text = "" + ms.value;
    }

    // Slider to set size n
    public void NSliderValue(Slider ns)
    {
        nValue = (int)ns.value;
        //Debug.Log("N Slider Value: " + nValue);
        GameObject.Find("TextNValueNum").GetComponent<Text>().text = "" + ns.value;
    }

    // Slider to set spacing between cubes
    public void SpacingSliderValue(Slider ss)
    {
        spacingValue = Mathf.Round(ss.value * 10f) / 10f; // get slider value to just 1 decimal point
        //Debug.Log("Delta Spacing value: " + spacingValue);
        GameObject.Find("TextDeltaSpaceNum").GetComponent<Text>().text = "" + spacingValue;
    }

    public void YMaxSliderValue(Slider ys)
    {
        heightValue = Mathf.Round(ys.value * 10f) / 10f; // get slider value to just 1 decimal point
        //Debug.Log("Delta Spacing value: " + spacingValue);
        GameObject.Find("TextYRangeNum").GetComponent<Text>().text = "" + heightValue;
    }

    public void HeightProgramSliderValue(Slider hs)
    {
        // get value and update UI
        //float heightValue = hs.value;
        float heightValue = Mathf.Round(hs.value * 10f) / 10f; // get slider value to just 1 decimal point
        GameObject.Find("TextHeightNum").GetComponent<Text>().text = "" + heightValue;

        // send the slider value to Node
        OnNodeProgramChanged(heightValue);
    }

    // Building platform using new values
    private void TopPanelUpdate()
    {
        // rebuild platform from values set in UI
        //platformSimManager.BuildPlatform(mValue, nValue, spacingValue);

        // updating UI for size and spacing (top panel)
        GameObject.Find("TextMxN").GetComponent<Text>().text = mValue + " x " + nValue;
        GameObject.Find("TextSpacingFloat").GetComponent<Text>().text = "" + spacingValue;
        GameObject.Find("TextYAxisNum").GetComponent<Text>().text = "" + heightValue;

        //Debug.Log("Green button clicked!");
    }

}

