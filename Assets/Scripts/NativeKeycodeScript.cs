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
    public AndroidJavaObject activity;
    public AndroidJavaObject communicationBridge;
    public AndroidJavaClass unityPlayer;


    public static NativeKeycodeScript instance;
    private Player player;


    private void Start()
    {
#if UNITY_ANDROID
        unityPlayer = new("com.unity3d.player.UnityPlayer");
        activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        communicationBridge = new AndroidJavaObject("com.Zigurous.SpaceInvaders.SetDataFromUnity");
#endif
    }

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

    public void NativeKeyCodes(string Keycode)
    {
        switch (Keycode)
        {
            case "1": // left Movement
                player?.MoveLeft();
                break;

            case "2": //right Movement
                player?.MoveRight();
                break;

            case "3":
                GameManager.Instance.gameOverUI.SetActive(false); //spacebar close first gesture
                break;
            case "4":
                GameManager.Instance.raiseHandPanel.SetActive(false); //Raise your healthy hand to begin! gesture to close panel 
                GameManager.Instance.StartLevel();
                break;
        }

    }

}
