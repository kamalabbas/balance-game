using UnityEngine;

public class SessionPrefsResetOnQuit : MonoBehaviour
{
    private const string TimeLimitPrefKey = "TimeLimitSeconds";
    private const string ModePrefKey = "MenuSelectedModeLabel";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        var go = new GameObject(nameof(SessionPrefsResetOnQuit));
        DontDestroyOnLoad(go);
        go.AddComponent<SessionPrefsResetOnQuit>();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey(TimeLimitPrefKey);
        PlayerPrefs.DeleteKey(ModePrefKey);
        PlayerPrefs.Save();
    }
}
