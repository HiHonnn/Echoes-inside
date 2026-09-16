using UnityEngine;

public class PuzzleHotspot : MonoBehaviour
{
    [Header("UI Panel cận cảnh cần mở")]
    [SerializeField] private GameObject closeUpPanel;

    [Header("Panel tổng (để ẩn khi vào cận cảnh)")]
    [SerializeField] private GameObject mainRoomView;

    public void OpenCloseUp()
    {
        if (closeUpPanel != null)
        {
            closeUpPanel.SetActive(true);
        }

        // Ẩn các hotspot của phòng chính khi đang xem cận cảnh
        if (mainRoomView != null)
        {
            mainRoomView.SetActive(false);
        }
    }
}
