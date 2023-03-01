using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Threading.Tasks;
using System;
using Cysharp.Threading.Tasks;

public class LoginManager
{
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
    /// 初期化処理
    /// </summary>
    /// <returns></returns>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static async UniTaskVoid InitializeAsync()
    {
        Debug.Log("初期化開始");

        await PreparateLoginPlayFab();

        Debug.Log("初期化完了");
    }

    public static async UniTask PreparateLoginPlayFab()
    {
        Debug.Log("ログイン準備開始");

        await LoginAndUpdateLocalCacheAsync();　　　　

        ////仮のログイン情報(リクエスト)を作成して設定
        //var request = new LoginWithCustomIDRequest
        //{
        //    CustomId = "GettingStartedGuide",　//この部分がユーザーIDになります
        //    CreateAccount = true　//アカウントが作成されない場合、trueの場合は匿名ログインとしてアカウントを作成する
        //};

        ////PlayFabへのログイン。情報が確認できるまで待機
        //var result = await PlayFabClientAPI.LoginWithCustomIDAsync(request);

        ////エラー内容を見て、ログインに成功しているか判定(エラーハンドリング)
        //var message = result.Error is null
        //    ? $"ログイン成功！ My PlayFab is{result.Result.PlayFabId}" //Errorがnullならば{エラーなし}なのでログイン成功
        //    : result.Error.GenerateErrorReport();　//Errorがnullでなければエラーが発生しているのでレポート作成

        //Debug.Log(message);
    }

    /// <summary>
    /// ユーザーデータとタイトルデータを初期化
    /// </summary>
    /// <returns></returns>
    public static async UniTask LoginAndUpdateLocalCacheAsync()
    {
        Debug.Log("初期化開始");

        //「UserId」の値を「PlayerPrefsManager」ヘルパークラスから取得
        var userId = PlayerPrefsManager.UserId;


        // ユーザーID が取得できない場合には新規作成して匿名ログインする
        // 取得できた場合には、ユーザーID を使ってログインする(次回の手順で実装するので TODO にしておく)
        // var の型は LoginResult 型(PlayFab SDK で用意されているクラス)

        //渡された文字列がnullまたは空文字列(文字数が0)の場合はtrue,それ以外の場合はfalseを返す。
        var loginResult = string.IsNullOrEmpty(userId)

            //IsNullOrEmpty(userId) が true の場合、新規ユーザーを作成し、匿名ログインを実行
            ? await CreateNewUserAsync()
            // falseの場合
            : await LoadUserAsync(userId);

        // TODO データを自動で取得する設定にしているので、取得したデータをローカルにキャッシュする
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
            CreateAccount = false　　　//　<=　アカウントの上書き処理は行わないようにする
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
}
