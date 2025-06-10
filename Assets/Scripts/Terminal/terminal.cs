using System.Collections.Generic;
using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace scener.ui {
    
public class Terminal : MonoBehaviour
{
    private ScrollView ui_scrollbox;
    private Label ui_label;
    readonly List<string> history = new();

    void Start(){
        //---------------------------

        //Search UI elements
        var uiDoc = FindFirstObjectByType<UIDocument>();
        if (uiDoc == null) return;
        var root = uiDoc.rootVisualElement;
        ui_scrollbox = root.Q("terminal").Q<ScrollView>("box_text");

        // Create a label 
        Font myFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        ui_label = new Label();
        ui_label.style.whiteSpace = WhiteSpace.Normal;
        ui_label.style.flexGrow = 1;
        ui_label.selection.isSelectable = true;
        ui_label.pickingMode = PickingMode.Position;
        ui_label.enableRichText = true;
        ui_label.style.unityFont = myFont;
        ui_label.text = "Welcome \n";
        ui_label.AddToClassList("label_text"); // Add USS element

        ui_scrollbox.Add(ui_label);

        //---------------------------
    }

    public void AddMessageToChat(string msg){
        //---------------------------

        if (ui_scrollbox == null){
            Debug.Log($"chat scroll view is null !");
            return;
        }

        ui_label.text += msg + "\n";
        ui_scrollbox.Add(ui_label);

        //---------------------------
    }

    public List<string> GetMessageHistory(){
        return new List<string>(history);
    }

}

}
