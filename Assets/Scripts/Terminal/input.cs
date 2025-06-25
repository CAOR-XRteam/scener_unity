using scener.ws;
using UnityEngine;
using UnityEngine.UIElements;

namespace ui.terminal
{
    public class TerminalInput : MonoBehaviour
    {
        private TextField field_text;
        private Button button_send;
        public TerminalLabel terminalDisplay;

        void Start()
        {
            //---------------------------

            //Search UI elements
            terminalDisplay = FindFirstObjectByType<TerminalLabel>();
            var root = FindFirstObjectByType<UIDocument>().rootVisualElement;
            field_text = root.Q("terminal")
                .Q<VisualElement>("box_input")
                .Q<TextField>("field_text");
            button_send = root.Q("terminal").Q<VisualElement>("box_input").Q<Button>("button_send");

            // Register callback
            field_text.RegisterCallback<KeyDownEvent>(OnTextFieldKeyDown);
            button_send.clicked += OnSendButtonClick;

            //---------------------------
        }

        private void OnTextFieldKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                evt.StopPropagation();
                SendMessageAndClear();
            }
        }

        private void OnSendButtonClick()
        {
            SendMessageAndClear();
        }

        private async void SendMessageAndClear()
        {
            string message = field_text.value.Trim();

            if (!string.IsNullOrEmpty(message))
            {
                await WsClient.instance.SendMessage("chat", message);

                terminalDisplay.AddMessageToChat("<b>[You]</b>: " + message);

                ClearTextField();
                field_text.Focus();
            }
        }

        public void ClearTextField()
        {
            field_text
                .schedule.Execute(() =>
                {
                    field_text.value = "";
                })
                .ExecuteLater(1);
        }
    }
}
