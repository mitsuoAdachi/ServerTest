using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

public class ATBController : MonoBehaviour
{
    private ReactiveProperty<float> currentAtb;

    private float maxAtb = 100;

    private float recoveryRate = 20;

    private float atbThreshold = 80;

    private bool isAnimating;

    [SerializeField]
    private Image imgAtbGauge;

    [SerializeField]
    private Button attackButton;

    // Start is called before the first frame updateb
    void Start()
    {
        // ATB値を0で初期化する
        currentAtb = new ReactiveProperty<float>(0f);

        //ATBゲージの値を更新する
        //Observable.EveryUpdate()
        //    .Where(_ => currentAtb.Value < maxAtb) //フィルター演算子
        //    .Subscribe(_ => currentAtb.Value += recoveryRate * Time.deltaTime)
        //    .AddTo(this);

        Observable.EveryUpdate()
        .Subscribe(_ =>
        {
            // ATBゲージの値を更新する
            if (currentAtb.Value < maxAtb)
            {
                currentAtb.Value += recoveryRate * Time.deltaTime;
            }

            // ATBゲージがMAXに達した時のアニメーション
            if (currentAtb.Value >= maxAtb && !isAnimating)
            {
                AnimateGaugeMax();
            }
        })
        .AddTo(this);

        //ATBゲージの値をImageに反映する
        currentAtb.Subscribe(x => imgAtbGauge.fillAmount = x / maxAtb)
            .AddTo(this);

        //ATBゲージが80以上になるとボタンを押せる状態にする
        currentAtb
            .Select(atb => atb >= atbThreshold)
            .SubscribeToInteractable(attackButton)
            .AddTo(this);

        //ATBゲージがMAXになった時のアニメーション
       // currentAtb
　　　　　　　//.SkipUntil(currentAtb.Where(x => x == maxAtb))
       //     .Take(1)
       //     .Subscribe(_ => AnimateGaugeMax())
       //     .AddTo(this);
    
    //ボタンを押すとイベントを発生してATBゲージをリセットする
    attackButton.OnClickAsObservable()
            .Subscribe(_ => Attack())
            .AddTo(this);      
    }

    /// <summary>
    /// 攻撃し、ATB値をリセットする
    /// </summary>
    private void Attack()
    {
        Debug.Log("攻撃");

        currentAtb.Value = 0;
    }

    /// <summary>
    /// ATBゲージがMAXになった時のアニメーション
    /// </summary>
    private void AnimateGaugeMax()
    {
        float duration = 0.5f;
        float scaleAmount = 1.2f;

        isAnimating = true;

        // ゲージが最大値に達した際の拡大・縮小アニメーション
        Sequence gaugeSequence = DOTween.Sequence();
        gaugeSequence.Append(imgAtbGauge.transform.DOScale(scaleAmount, duration))
                        .Append(imgAtbGauge.transform.DOScale(1, duration))
                        .SetLoops(2);

        // ゲージが最大値に達した際の回転アニメーション
        imgAtbGauge.transform.DORotate(new Vector3(0, 0, 360), duration, RotateMode.FastBeyond360)
                                .SetDelay(duration)
                                .OnComplete(() => isAnimating = false);
    }
}
