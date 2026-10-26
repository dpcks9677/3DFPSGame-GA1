using System;
using Lean.Pool;
using UnityEngine;

public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Bomb _bombPrefab;

    [SerializeField] private float _throwPower = 15f;
    private float _chargePower = 0f;

    [SerializeField] private const int MAX_BOMB = 10;
    [SerializeField] private int _remainingBombs = MAX_BOMB;

    private void Update()
    {
        bool isMouseVisible = Cursor.visible;

        if (!isMouseVisible && GameManager.Instance.State != GameState.Playing) { return; }

        if (Input.GetButton("Fire2"))
        {
            _chargePower += Time.deltaTime * 5;
        }

        // 1. 마우스 오른쪽 버튼 입력 받기
        if (Input.GetButtonUp("Fire2"))
        {
            Shoot();
            _chargePower = 0f;
        }
    }

    private void Shoot()
    {
        if (_remainingBombs > 0)
        {
            _remainingBombs--;

            // 2. 발사 위치에 수류탄 생성
            GameObject bomb = LeanPool.Spawn(_bombPrefab.gameObject, _firePoint.position, Quaternion.identity);

            // 3. 생성된 수류탄을 시선 방향으로 던지기
            Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();
            rigidbody.AddForce(Camera.main.transform.forward * (_throwPower + _chargePower), ForceMode.Impulse);
        }
    }
}
