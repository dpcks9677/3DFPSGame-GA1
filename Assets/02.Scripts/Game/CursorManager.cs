using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private void Awake()
    {
        LockCursor();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            UnlockCursor();
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            LockCursor();
        }
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 화면 가운데 고정
        Cursor.visible = false; // 마우스 커서 안 보이게 처리
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // 마우스 커서 화면 가운데 고정
        Cursor.visible = true; // 마우스 커서 안 보이게 처리
    }
}
