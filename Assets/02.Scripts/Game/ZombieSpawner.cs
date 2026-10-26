using System.Collections.Generic;
using Lean.Pool;
using UnityEngine;

// 스폰 지점 중 하나에서 일정 간격마다 좀비를 생성한다. 좀비는 LeanPool로 재사용한다.
public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _zombiePrefab;
    [SerializeField] private Transform[] _spawnPoints;

    [SerializeField] private float _spawnInterval = 3f;         // 생성 간격
    [SerializeField] private int _maxAlive = 10;                // 동시에 살아있을 수 있는 최대 좀비 수
    [SerializeField] private float _minPlayerDistance = 10f;    // 플레이어와 이보다 가까운 스폰 지점은 쓰지 않는다.

    private readonly List<Transform> _candidates = new List<Transform>();
    private Transform _player;
    private float _timer = 0f;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        _timer += Time.deltaTime;
        if (_timer < _spawnInterval) return;

        // 자리가 날 때까지(또는 쓸 수 있는 지점이 생길 때까지) 타이머를 유지했다가 바로 생성한다.
        if (ZombieManager.Instance.Count >= _maxAlive) return;

        if (!TryGetSpawnPoint(out Transform point)) return;

        LeanPool.Spawn(_zombiePrefab, point.position, point.rotation);
        _timer = 0f;
    }

    private bool TryGetSpawnPoint(out Transform point)
    {
        _candidates.Clear();
        foreach (Transform spawnPoint in _spawnPoints)
        {
            if (Vector3.Distance(spawnPoint.position, _player.position) >= _minPlayerDistance)
            {
                _candidates.Add(spawnPoint);
            }
        }

        point = _candidates.Count > 0 ? _candidates[Random.Range(0, _candidates.Count)] : null;
        return point != null;
    }
}
