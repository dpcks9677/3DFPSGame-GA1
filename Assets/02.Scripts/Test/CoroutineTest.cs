using System.Collections;
using UnityEngine;


// 코루틴 -> 비동기 실행과 제어를 효율적으로 제공하는 기능
// 1. 비동기 실행
// 2. 시간/프레임에 따른 실행 제어

// IEnumerator: 코루틴 함수 반환형
// StartCoroutine(somethingMethod()): 유니티에서 코루틴 실행하는 함수
// yield: 정해진 시간/프레임동안 쉼

// 비동기 함수 -> 작업이 완료되지 않더라도 다음 코드를 실행함

public class CoroutineTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("시작");

        StartCoroutine(Countdown_Coroutine());

        Debug.Log("끝");
    }

    private IEnumerator Countdown_Coroutine()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("3...");

        yield return new WaitForSeconds(1f);
        Debug.Log("2...");

        yield return new WaitForSeconds(1f);
        Debug.Log("1...");

        yield return new WaitForSeconds(1f);
        Debug.Log("발사!");
    }
}
