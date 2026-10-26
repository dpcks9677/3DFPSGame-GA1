using UnityEngine;
using UnityEngine.UI;

public class ZombieHealthBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    private Zombie _zombie;
    private Transform _canvasTransform;

    private void Awake()
    {
        _zombie = GetComponent<Zombie>();
        _canvasTransform = _slider.GetComponentInParent<Canvas>().transform;
    }

    private void Start()
    {
        _zombie.OnDamaged += RefreshHealth;
        RefreshHealth();
    }

    // 풀에서 다시 꺼내질 때 Start는 다시 실행되지 않으므로 체력바를 갱신한다.
    private void OnEnable()
    {
        if (_zombie != null)
        {
            RefreshHealth();
        }
    }

    private void RefreshHealth()
    {
        // 0 ~ 100% -> 0 ~ 1
        // _slider.value = 좀비의 현재 체력 / 좀비의 최대 체력
        _slider.value = _zombie.Health / _zombie.MaxHealth;
    }

    private void LateUpdate()
    {
        // 빌보드: 체력바가 카메라를 쳐다보게한다.
        _canvasTransform.forward = Camera.main.transform.forward;
    }
}