using Scener.Exporter;
using Scener.Sdk;
using Scener.Ws;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ui.Terminal
{
    public class TerminalInput : MonoBehaviour
    {
        private TextField field_text;
        private Button button_send;
        public TerminalLabel terminalDisplay;
        public SceneSerializer sceneSerializer;

        void Start()
        {
            //---------------------------

            //Search UI elements
            terminalDisplay = FindFirstObjectByType<TerminalLabel>();
            var root = FindFirstObjectByType<UIDocument>().rootVisualElement;
            field_text = root.Q<TextField>("field_text");
            button_send = root.Q<Button>("button_send");

            // Register callback
            field_text.RegisterCallback<KeyDownEvent>(OnChatInputKeyDown);
            button_send.clicked += CallbackButton;

            sceneSerializer = FindFirstObjectByType<SceneSerializer>();

            //---------------------------
        }

        private void OnChatInputKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                evt.StopPropagation();
                SendMessageAndClear();
            }
        }

        private void CallbackButton()
        {
            SendMessageAndClear();
        }

        private async void SendMessageAndClear()
        {
            string message = field_text.value.Trim();

            if (!string.IsNullOrEmpty(message))
            {
                var textMessage = new OutgoingTextMessage(message);

                await WsClient.instance.SendMessage(textMessage);

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
