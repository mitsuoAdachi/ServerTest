using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Threading.Tasks;
using System;
using Cysharp.Threading.Tasks;

public static class LoginManager
{
    /// <summary>
    /// ログインと同時に PlayFab から取得する情報の設定用クラスである GetPlayerCombinedInfoRequestParams のプロパティ。
    /// GetPlayerCombinedInfoRequestParams クラスで設定した値が InfoRequestParameters の設定値になり、true にしてある項目で各情報が自動的に取得できるようになる
    /// 各パラメータの初期値はすべて false
    /// 取得が多くなるほどログイン時間がかかり、メモリを消費するので気を付ける
    /// 取得結果は InfoResultPayLoad に入っている。false のものはすべて null になる
    /// </summary>
    public static GetPlayerCombinedInfoRequestParams CombinedInfoRequestParams { get; }
        = new GetPlayerCombinedInfoRequestParams
        {
            GetUserAccountInfo = true,
            GetPlayerProfile = true,
            GetTitleData = true,
            GetUserData = true,
            GetUserInventory = true,
            GetUserVirtualCurrency = true,
            GetPlayerStatistics = true
        };

    /// <summary>
    /// コンストラクタ
    /// </summary>
    static LoginManager()
    {
        //TitleID設定　"PlayFab で作成したタイトルのID"
        PlayFabSettings.staticSettings.TitleId = "3FB29";

        Debug.Log("TitleID設定" + PlayFabSettings.staticSettings.TitleId);
    }

    /// <summary>
    /// ユーザーデータとタイトルデータを初期化
    /// </summary>
    /// <returns></returns>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]　//ゲーム開始時に自動的に１回のみ呼び出される。
    public static async UniTask LoginAndUpdateLocalCacheAsync()
    {
        Debug.Log("初期化開始");

        // ユーザーID が取得できない場合には新規作成して匿名ログインする
        // 取得できた場合には、ユーザーID を使ってログインする(次回の手順で実装するので TODO にしておく)
        // var の型は LoginResult 型(PlayFab SDK で用意されているクラス)

        //渡された文字列がnullまたは空文字列(文字数が0)の場合はtrue,それ以外の場合はfalseを返す。
        var loginResult = string.IsNullOrEmpty(PlayerPrefsManager.UserId)

            //IsNullOrEmpty(userId) が true の場合、新規ユーザーを作成し、匿名ログインを実行
            ? await CreateNewUserAsync()
            // falseの場合
            : await LoadUserAsync(PlayerPrefsManager.UserId);

            //プレイヤーデータの作成と更新
              //await CreateUserDataAsync();

        // PlayFab のデータを自動で取得する設定にしているので、取得したデータをローカルにキャッシュする
        UpdateLocalCacheAsync(loginResult);    //  <=  後程、async メソッドになったら await を追加します。
    }

    /// <summary>
    /// 新規ユーザーを作成して UserId を PlayerPrefs に保存
    /// </summary>
    /// <returns></returns>
    private static async UniTask<LoginResult> CreateNewUserAsync()
    {
        Debug.Log("ユーザーデータなし。新規ユーザー作成");

        while (true)
        {
            //UserIdの採番
            //GUIDを文字列として生成し、文字列からハイフンを削除し、その文字列から最初の20文字を取り出す。
            var newUserId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);

            // ログインリクエストの作成(以下の処理は、いままでPrepareLoginPlayPabメソッド内に書いてあったものを修正して記述)
            var request = new LoginWithCustomIDRequest
            {
                CustomId = newUserId, //  <=  ここが前の処理と異なる
                CreateAccount = true,

                InfoRequestParameters = CombinedInfoRequestParams　　　// プロパティの情報を設定
            };

            //PlayFabにログイン
            var respones = await PlayFabClientAPI.LoginWithCustomIDAsync(request);

            //エラーハンドリング
            if(respones.Error != null)
            {
                Debug.Log("Error");
                respones.Error.GenerateErrorReport();
            }

            // もしもLastLoginTimeに値が入っている場合には、採番したIDが既存ユーザーと重複しているのでリトライする
            if (respones.Result.LastLoginTime.HasValue)
            {
                continue;
            }

            //PlayPrefsにUserIdを記録する
            PlayerPrefsManager.UserId = newUserId;

            return respones.Result;
        }
    }

    /// <summary>
    /// ログインしてユーザーデータをロード
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private static async UniTask<LoginResult> LoadUserAsync(string userId)
    {
        Debug.Log("ユーザーデータあり。ログイン開始");

        // ログインリクエストの作成
        var request = new LoginWithCustomIDRequest
        {
            CustomId = userId,
            CreateAccount = false,　　//　<=　アカウントの上書き処理は行わないようにする

            InfoRequestParameters = CombinedInfoRequestParams　　　// プロパティの情報を設定
        };

        // PlayFab にログイン
        var response = await PlayFabClientAPI.LoginWithCustomIDAsync(request);

        // エラーハンドリング
        if (response.Error != null)
        {
            Debug.Log("Error");

            // TODO response.Error にはエラーの種類が値として入っている
            // そのエラーに対応した処理を switch 文などで記述して複数のエラーに対応できるようにする
        }

        // エラーの内容を見てハンドリングを行い、ログインに成功しているかを判定
        var message = response.Error is null
            ? $"Login success! My PlayFabID is {response.Result.PlayFabId}"
            : response.Error.GenerateErrorReport();

        Debug.Log(message);

        return response.Result;
    }

    /// <summary>
    /// Email とパスワードでログイン(アカウント回復用)
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static async UniTask<(bool, string)> LoginEmailAndPasswordAsync(string email, string password)
    {

        // Email によるログインリクエストの作成
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
            InfoRequestParameters = CombinedInfoRequestParams
        };

        // PlayFab にログイン
        var response = await PlayFabClientAPI.LoginWithEmailAddressAsync(request);

        // エラーハンドリング
        if (response.Error != null)
        {
            switch (response.Error.Error)
            {
                case PlayFabErrorCode.InvalidParams:
                case PlayFabErrorCode.InvalidEmailOrPassword:
                case PlayFabErrorCode.AccountNotFound:
                    Debug.Log("メールアドレスかパスワードが正しくありません");
                    break;
                default:
                    Debug.Log(response.Error.GenerateErrorReport());
                    break;
            }

            return (false, "メールアドレスかパスワードが正しくありません");
        }

        // PlayerPrefas を初期化して、ログイン結果の UserId を登録し直す
        PlayerPrefs.DeleteAll();

        // 新しく PlayFab から UserId を取得
        // InfoResultPayload はクライアントプロフィールオプション(InfoRequestParameters)で許可されてないと null になる
        PlayerPrefsManager.UserId = response.Result.InfoResultPayload.AccountInfo.CustomIdInfo.CustomId;

        // Email でログインしたことを記録する
        PlayerPrefsManager.IsLoginEmailAdress = true;

        return (true, "Email によるログインが完了しました。");
    }

    /// <summary>
    /// PlayFab から取得したデータ群をローカル(端末)にキャッシュ
    /// </summary>
    /// <param name="loginResult"></param>
    /// <returns></returns>
    public static void UpdateLocalCacheAsync(LoginResult loginResult)
    {    //  <=　後程、async を追加し、戻り値を UniTask に変更します。

        // TODO カタログ類の初期化。他のインスタンスの初期化にも必要なので最初に行う


        // TODO タイトルデータの取得


        // ユーザーデータの取得
        UserDataManager.SyncPlayFabToClient(loginResult.InfoResultPayload.UserData);


        // ユーザー名などの取得
        PlayerPlofileManager.SyncPlayFabToClient(loginResult.InfoResultPayload.PlayerProfile, loginResult.InfoResultPayload.PlayerStatistics);


        // TODO 他の初期化処理を追加


        Debug.Log("各種データのキャッシュ完了");
    }

    /// <summary>
    /// プレイヤーデータの作成と更新。デバッグ用
    /// </summary>
    private static async UniTask CreateUserDataAsync()
    {
        //var createData = new Dictionary<string, string> {
        //    { "Level", "0"}
        //};

        //await UserDataManager.UpdatePlayerDataAsync(createData);　　//　ここまでコメントアウトします


        UserDataManager.User = User.Create();      //  新しく User 作成します
        string key = "User";　　　　　　　　　　　 //　保存用の Key を作成します

        await UserDataManager.UpdateUserDataByJsonAsync(key);　　　// PlayFab に Json 形式にした User クラスの情報を登録します

        Debug.Log("ユーザーデータ 登録完了");
    }
}
