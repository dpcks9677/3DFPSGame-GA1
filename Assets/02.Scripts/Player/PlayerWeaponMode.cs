using System;
using UnityEngine;


public class PlayerWeaponMode : MonoBehaviour
{
    private WeaponMode _mode = WeaponMode.Normal;
    public WeaponMode Mode => _mode;

    public event Action<WeaponMode> OnWeaponModeChanged;  // 무기 모드가 바뀌면 새 모드를 알린다.

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeMode(WeaponMode.Normal);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeMode(WeaponMode.Sniper);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeMode(WeaponMode.Melee);
        }
    }

    // 무기 모드를 바꾸고, 바뀌었다고 알린다.
    private void ChangeMode(WeaponMode mode)
    {
        _mode = mode;
        OnWeaponModeChanged?.Invoke(mode);
        Debug.Log($"무기 모드: {mode}");
    }
}
