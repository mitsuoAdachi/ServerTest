using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

public class DisplayNameCanvas : MonoBehaviour
{
    [SerializeField]
    private Button btnSubmit;

    [SerializeField]
    private Button btnCancel;

    [SerializeField]
    private InputField displayNameInput;

    [SerializeField]
    private Text txtDisplayName;

    [SerializeField]
    private GameObject responsePopUp;

    [SerializeField]
    private Button btnClosePopUp;

    [SerializeField]
    private Text txtResponseInfo;

    private string displayName;　　// ユーザー名登録用


    void Start()
    {
        responsePopUp.SetActive(false);

        // ボタンの登録
        btnSubmit?.OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(0.5f))
            .Subscribe(_ => OnClickSubmit());

        btnCancel?.OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(0.5f))
            .Subscribe(_ => OnCliclCancel());

        btnClosePopUp?.OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(0.5f))
            .Subscribe(_ => OnClickCloseCompletePopUp());

        // InputField(文字入力を監視し、画面の表示更新も行う)
        displayNameInput?.OnEndEditAsObservable()
            .Subscribe(x => UpdateDispayName(x));
    }

    /// <summary>
    /// ユーザー名の値と表示の更新
    /// </summary>
    /// <param name="newName"></param>
    private void UpdateDispayName(string newName)
    {
        txtDisplayName.text = newName;
        displayName = newName;
        Debug.Log(displayName);
    }

    /// <summary>
    /// OK ボタンを押下した際の処理
    /// </summary>
    private async void OnClickSubmit()
    {

        Debug.Log("OK アカウント連携の承認開始");

        // Email とパスワードを利用して、ユーザーアカウントの連携を試みる
        (bool isSuccess, string message) response = await PlayerPlofileManager.UpdateUserDisplayNameAsync(displayName);

        // Debug用
        if (response.isSuccess)
        {
            Debug.Log("ユーザー名　更新成功");
        }
        else
        {
            Debug.Log("ユーザー名　更新失敗");
        }

        txtResponseInfo.text = response.message;
        responsePopUp.SetActive(true);

    }

    /// <summary>
    /// NG ボタンを押下した際の処理
    /// </summary>
    private void OnCliclCancel()
    {
        this.gameObject.SetActive(false);

        Debug.Log("NG");
    }

    /// <summary>
    /// CompletePopUp をタップした際の処理
    /// </summary>
    private void OnClickCloseCompletePopUp()
    {

        responsePopUp.SetActive(false);

        this.gameObject.SetActive(false);
    }
}
