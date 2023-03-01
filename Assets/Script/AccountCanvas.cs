using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class AccountCanvas : MonoBehaviour
{
    [SerializeField]
    private Button btnSubmit;

    [SerializeField]
    private Button btnCancel;

    [SerializeField]
    private InputField emailInput;

    [SerializeField]
    private InputField passwordInput;

    [SerializeField]
    private Text txtEmail;

    [SerializeField]
    private Text txtPassword;

    [SerializeField]
    private GameObject responsePopUp;

    [SerializeField]
    private Button btnClosePopUp;

    [SerializeField]
    private Button btnEmainLogin;

    [SerializeField]
    private Text txtResponseInfo;

    private (string email, string pasword) inputValue;　　// Email とパスワード登録用


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

        btnEmainLogin?.OnClickAsObservable()
            .ThrottleFirst(TimeSpan.FromSeconds(0.5f))
            .Subscribe(_ => OnClickEmailLogin());

        // InputField
        emailInput?.OnEndEditAsObservable()
            .Subscribe(x => UpdateDispayEmail(x));

        passwordInput?.OnEndEditAsObservable()
            .Subscribe(x => UpdateDisplayPassword(x));
    }

    /// <summary>
    /// Email の値と表示の更新
    /// </summary>
    /// <param name="newEmail"></param>
    private void UpdateDispayEmail(string newEmail)
    {
        txtEmail.text = newEmail;
        inputValue.email = newEmail;
        Debug.Log(inputValue);
    }

    /// <summary>
    /// パスワードの値と表示の更新
    /// </summary>
    /// <param name="newPassword"></param>
    private void UpdateDisplayPassword(string newPassword)
    {
        txtPassword.text = newPassword;
        inputValue.pasword = newPassword;
        Debug.Log(inputValue);
    }

    /// <summary>
    /// OK ボタンを押下した際の処理
    /// </summary>
    private async void OnClickSubmit()
    {
        Debug.Log("OK アカウント連携の承認開始");

        // Email とパスワードを利用して、ユーザーアカウントの連携を試みる
        bool isLink = await PlayFabAccountLink.SetEmailAndPasswordAsync(inputValue.email, inputValue.pasword);

        if (isLink)
        {
            Debug.Log("連携完了");

            txtResponseInfo.text = "アカウント連携が完了しました。";
            responsePopUp.SetActive(true);
        }
        else
        {
            Debug.Log("アカウント連携が失敗しました。");

            txtResponseInfo.text = "アカウント連携が失敗しました。";
            responsePopUp.SetActive(true);
        }
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

    /// <summary>
    /// Email でログインボタンを押下した際の処理
    /// </summary>
    private async void OnClickEmailLogin()
    {

        // Email でログインを試みる isLogin = true ならログイン成功
        // TODO 次の手順で LoginManager に該当するメソッドを作成するので、それまでコメントアウトしておく(エラーが出るため）
        //(bool isLogin, string log) response = await LoginManager.LoginEmailAndPasswordAsync(inputValue.email, inputValue.pasword);

        // TODO isLogin で分岐させてもいい

        //Debug.Log(response.log);

        //txtResponseInfo.text = response.log;
        responsePopUp.SetActive(true);
    }
}