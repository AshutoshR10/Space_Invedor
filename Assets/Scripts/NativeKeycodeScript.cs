using TMPro;
using UnityEngine;


#if UNITY_IOS
using System.Runtime.InteropServices;
using JetBrains.Annotations;

#endif

using UnityEngine.Android;

#if UNITY_IOS
public class NativeAPI
{
    [DllImport("__Internal")]
    public static extern void sendMessageToMobileApp(string message);
}
#endif

public class NativeKeycodeScript : MonoBehaviour
{
   /* public AndroidJavaObject activity;
    public AndroidJavaObject communicationBridge;
    public AndroidJavaClass unityPlayer;*/


    public static NativeKeycodeScript instance;
    private Player player;

    public TMP_Text debugText;


    /*private void Start()
    {
#if UNITY_ANDROID
        unityPlayer = new("com.unity3d.player.UnityPlayer");
        activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        communicationBridge = new AndroidJavaObject("com.Zigurous.SpaceInvaders.SetDataFromUnity");
#endif
    }*/

    private void Awake()
    {
        player = FindFirstObjectByType(typeof(Player)) as Player;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ShowDebug(string message)
    {
        Debug.Log(message); // still log to console
        if (debugText != null)
            debugText.text = message;
    }

    public void NativeKeyCodes(string Keycode)
    {
        switch (Keycode)
        {
            case "2": // left Movement
                player?.MoveLeft();
                ShowDebug("Key 2 pressed  Player Move Left");
                break;

            case "3": //right Movement
                player?.MoveRight();
                ShowDebug("Key 3 pressed  Player Move Right");
                break;

            case "4":
                //GameManager.Instance.gameOverUI.SetActive(false); //spacebar close first gesture
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.gameOverUI.SetActive(false);
                }
                ShowDebug("Key 4 pressed  Closed Game Over UI");
                break;
            case "5":
                GameManager.Instance.raiseHandPanel.SetActive(false); //Raise your healthy hand to begin! gesture to close panel 
                GameManager.Instance.StartLevel();
                ShowDebug("Key 5 pressed  Started Level (Raise Hand Closed)");
                break;
        }

    }

}
