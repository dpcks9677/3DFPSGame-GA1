using UnityEngine;

public class CursorManager : MonoBehaviour
{
    private void Awake()
    {
        LockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 화면 가운데 고정
        Cursor.visible = false; // 마우스 커서 안 보이게 처리
    }

    private void unlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // 마우스 커서 화면 가운데 고정
        Cursor.visible = true; // 마우스 커서 안 보이게 처리
    }
}
