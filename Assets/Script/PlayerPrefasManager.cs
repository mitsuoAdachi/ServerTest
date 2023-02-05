using UnityEngine;

/// <summary>
/// PlayerPrefsのヘルパークラス
/// </summary>
public static class PlayerPrefsManager
{
    //プロパティとしてUserIdを作成する
    public static string UserId
    {
        set
        {
            PlayerPrefs.SetString("UserId", value);
            PlayerPrefs.Save();
        }
        get => PlayerPrefs.GetString("UserId");
    }
}
