using System.Collections;
using UnityEditor;

public class PopUpWindow : EditorWindow {

    public static void ShowWindow()
    {
    GetWindow<PopupWindow>("PopUp");
    }

    void OnGUI()
    {

    }
}
