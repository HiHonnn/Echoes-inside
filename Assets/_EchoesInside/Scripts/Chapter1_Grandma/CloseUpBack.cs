using UnityEngine;

public class CloseUpBack : MonoBehaviour
{
    [Header("Panel cận cảnh này (sẽ bị ẩn khi quay lại)")]
    [SerializeField] private GameObject thisPanel;

    [Header("Panel phòng chính (sẽ hiện lại khi quay lại)")]
    [SerializeField] private GameObject mainRoomView;

    public void GoBack()
    {
        // Ẩn panel cận cảnh
        if (thisPanel != null)
        {
            thisPanel.SetActive(false);
        }

        // Hiện lại phòng chính
        if (mainRoomView != null)
        {
            mainRoomView.SetActive(true);
        }
    }
}
