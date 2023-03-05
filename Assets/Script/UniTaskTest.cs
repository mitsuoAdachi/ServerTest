using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UniTaskTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("UniTask待機");

        StartCoroutine(UniTaskTest2());

        Debug.Log("次の処理実行");

    }

    private IEnumerator UniTaskTest2()
    {
        yield return new WaitForSeconds(5);

        Debug.Log("UniTask完了");
    }
}
