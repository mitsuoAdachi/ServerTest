using UnityEngine;

public class User
{
    public int level;
    public bool tutorialFlag;

    // TODO 他にも管理したいユーザー情報があれば追加


    /// <summary>
    /// 新規ユーザーの作成
    /// </summary>
    /// <returns></returns>
    public static User Create()
    {
        User user = new User
        {
            level = 0,
            tutorialFlag = false

            // TODO 変数を追加した場合には、この部分で変数の初期化を行う
        };

        Debug.Log("新規ユーザーの作成");

        return user;
    }
}
