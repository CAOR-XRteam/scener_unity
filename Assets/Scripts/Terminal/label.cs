using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ui.Terminal
{
    public class TerminalLabel : MonoBehaviour
    {
        private ScrollView ui_scrollbox;
        private Label ui_label;
        readonly List<string> history = new();

        void Start()
        {
            //Init label ui
            //---------------------------

            //Search UI elements
            var uiDoc = FindFirstObjectByType<UIDocument>();
            if (uiDoc == null)
            {
                Debug.LogError("UIDocument not found!");
                return;
            }
            var root = uiDoc.rootVisualElement;
            ui_scrollbox = root.Q("terminal").Q<ScrollView>("box_text");
            ui_scrollbox.contentContainer.style.paddingTop = 10;
            ui_scrollbox.contentContainer.style.paddingBottom = 20;

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

        public void AddMessageToChat(string msg)
        {
            //Add an entry to the terminal text
            //---------------------------

            if (ui_scrollbox == null)
            {
                Debug.Log($"chat scroll view is null !");
                return;
            }

            //Add formatted message
            ui_label.text += msg + "\n";
            ui_scrollbox.Add(ui_label);

            //Scroll to bottom
            float scrollToY =
                ui_scrollbox.contentContainer.layout.height - ui_scrollbox.layout.height + 20;
            if (scrollToY < 0)
                scrollToY = 0;
            ui_scrollbox.scrollOffset = new Vector2(0, scrollToY);

            //---------------------------
        }

        public List<string> GetMessageHistory()
        {
            return new List<string>(history);
        }
    }
}
