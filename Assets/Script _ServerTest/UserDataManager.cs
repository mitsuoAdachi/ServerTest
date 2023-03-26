using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;


public static class UserDataManager
{
    public static User User { get; set; }

    // TODO Level などの情報を持たせる

    /// <summary>
    /// プレイヤーデータ内の作成と更新(プレイヤーデータ(タイトル)の Key に１つだけ値を登録する方法)
    /// </summary>
    /// <param name="updateUserData"></param>
    /// <param name="userDataPermission"></param>
    public static async UniTask UpdatePlayerDataAsync(Dictionary<string, string> updateUserData, UserDataPermission userDataPermission = UserDataPermission.Private)
    {
        var request = new UpdateUserDataRequest
        {
            Data = updateUserData,

            // アクセス許可の変更
            Permission = userDataPermission
        };

        var response = await PlayFabClientAPI.UpdateUserDataAsync(request);

        if (response.Error != null)
        {
            Debug.Log("エラー");
            return;
        }

        Debug.Log("プレイヤーデータ　更新");
    }

    /// <summary>
    /// プレイヤーデータから指定した Key の情報の削除
    /// </summary>
    /// <param name="deleteKey">削除する Key の名前</param>
    public static async void DeletePlayerDataAsync(string deleteKey)
    {
        var request = new UpdateUserDataRequest
        {
            //KeysToRemoveがList型のためListで作成
            KeysToRemove = new List<string> { deleteKey }
        };

        var response = await PlayFabClientAPI.UpdateUserDataAsync(request);

        if (response.Error != null)
        {
            Debug.Log("エラー");
            return;
        }

        Debug.Log("プレイヤーデータ　削除");
    }

    /// <summary>
    /// プレイヤーデータの作成と更新(プレイヤーデータ(タイトル)の Key に複数の値情報をまとめた Json を利用する場合)
    /// </summary>
    /// <param name="userName">key</param>
    /// <param name="userDataPermission"></param>
    /// <returns></returns>
    public static async UniTask<(bool isSuccess, string errorMessage)> UpdateUserDataByJsonAsync(string userName, UserDataPermission userDataPermission = UserDataPermission.Private)
    {
        string userJson = JsonConvert.SerializeObject(User);　　　//　<=　この機能が Json.NET ライブラリの処理です。

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {
                { userName, userJson }
            },

            // アクセス許可の変更
            Permission = userDataPermission
        };

        var response = await PlayFabClientAPI.UpdateUserDataAsync(request);

        if (response.Error != null)
        {
            Debug.Log("エラー");
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// PlayFab の最新データを取得してローカルにキャッシュ
    /// </summary>
    /// <param name="userData"></param>
    public static void SyncPlayFabToClient(Dictionary<string, UserDataRecord> userData)
    {
        // ユーザーの情報を取得。取得できた場合には複合化、取得できない場合には新規ユーザーの作成
        User = userData.TryGetValue("User", out var user)
            ? JsonConvert.DeserializeObject<User>(user.Value) : User.Create();

        Debug.Log("PlayFab のユーザーデータを取得");

        // TODO 他にも処理があれば追加
    }

}