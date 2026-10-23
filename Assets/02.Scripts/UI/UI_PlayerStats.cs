using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private PlayerMove _playerMove;

    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;

    private void LateUpdate()
    {
        // 0 ~ 100% -> 0 ~ 1
        // _slider.value = 좀비의 현재 체력 / 좀비의 최대 체력
        _healthSlider.value = _player.Health / _player.MaxHealth;
        _staminaSlider.value = _playerMove.Stamina / _playerMove.MaxStamina;
    }
}
