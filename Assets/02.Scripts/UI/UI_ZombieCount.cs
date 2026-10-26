using TMPro;
using UnityEngine;

public class UI_ZombieCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    private void Start()
    {
        ZombieManager.Instance.OnCountChanged += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        _text.text = $"남은 좀비: {ZombieManager.Instance.Count}";
    }
}
