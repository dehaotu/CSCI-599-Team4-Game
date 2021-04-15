using System;
using UnityEngine;

namespace Mirror
{
    /// <summary>
    /// Component that will display the clients ping in milliseconds
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkPingDisplay")]
    [HelpURL("https://mirror-networking.com/docs/Articles/Components/NetworkPingDisplay.html")]
    public class NetworkPingDisplay : MonoBehaviour
    {
        public Color color = Color.white;
        public int padding = 2;
        int width = 150;
        int height = 25;

        void OnGUI()
        {
            // only while client is active
            if (!NetworkClient.active) return;

            // show rtt in bottom right corner, right aligned
            GUI.color = color;
            Rect rect = new Rect(Screen.width / 2, Screen.height / 2, width, height);
            GUIStyle newStyle = new GUIStyle();
            newStyle.alignment = TextAnchor.MiddleRight;
            newStyle.normal.textColor = Color.white;
            newStyle.fontSize = 50;
            GUI.Label(rect, $"RTT: {Math.Round(NetworkTime.rtt * 1000)}ms", newStyle);
            GUI.color = Color.white;
        }
    }
}
