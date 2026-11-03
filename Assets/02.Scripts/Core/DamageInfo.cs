using UnityEngine;

public struct DamageInfo
{
    public float Value;            // 대미지 양
    public Vector3 HitPoint;       // 맞은 자리
    public Vector3 HitNormal;      // 맞은 자리가 수직으로 바라보는 방향
    public float Knockback;        // 밀려나는 거리(m). 0이면 받는 쪽의 기본값을 쓴다.
}