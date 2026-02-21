using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MobileUIButtons : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Sprint Button")]
    [SerializeField] private Image sprintButtonImage;
    [SerializeField] private Text sprintLabel;
    [SerializeField] private Color sprintOnColour = new Color(1f, 0.55f, 0f, 1f);   // orange = active;
    [SerializeField] private Color sprintOffColour = new Color(1f, 1f, 1f, 0.55f); // faded  = inactive;
    [SerializeField] private string sprintOnText = "SPRINT ✓";
    [SerializeField] private string sprintOffText = "SPRINT";

    [Header("Jump Button")]
    [SerializeField] private Image jumpButtonImage;
    [SerializeField] private Color jumpNormalColour = new Color(1f, 1f, 1f, 0.55f);
    [SerializeField] private Color jumpPressedColour = new Color(0.6f, 0.9f, 1f, 1f);  // light blue flash;

    private bool isSprinting = false;

    private void Start()
    {
        RefreshSprintVisual();
        if (jumpButtonImage != null)
            jumpButtonImage.color = jumpNormalColour;
    }

    // sprint toggle on/off;
    public void OnSprintToggled()
    {
        isSprinting = !isSprinting;
        playerController?.SetSprinting(isSprinting);
        RefreshSprintVisual();
    }

    private void RefreshSprintVisual()
    {
        if (sprintButtonImage != null)
            sprintButtonImage.color = isSprinting ? sprintOnColour : sprintOffColour;

        if (sprintLabel != null)
            sprintLabel.text = isSprinting ? sprintOnText : sprintOffText;
    }

    // tap to jump, brief colour flash for feedback;

    public void OnJumpPressed()
    {
        playerController?.OnJumpButton();
        StartCoroutine(FlashJump());
    }

    private IEnumerator FlashJump()
    {
        if (jumpButtonImage != null) jumpButtonImage.color = jumpPressedColour;
        yield return new WaitForSeconds(0.12f);
        if (jumpButtonImage != null) jumpButtonImage.color = jumpNormalColour;
    }
}