using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;

public static class UserDataManager
{
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
            //KeysToRemoveがList型のため
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
}