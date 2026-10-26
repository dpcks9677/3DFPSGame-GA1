using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private PlayerMove _playerMove;

    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;

    private void Start()
    {
        _player.OnDamaged += RefreshHealth;
        RefreshHealth();
    }

    private void LateUpdate()
    {
        // 0 ~ 100% -> 0 ~ 1
        // _slider.value = 좀비의 현재 체력 / 좀비의 최대 체력
        // RefreshHealth();
        _staminaSlider.value = _playerMove.Stamina / _playerMove.MaxStamina;
    }

    private void RefreshHealth()
    {
        _healthSlider.value = _player.Health / _player.MaxHealth;
    }
}
