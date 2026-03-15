using UnityEngine;

public class EnemyApproachLabController : LabControllerBase
{
    [Header("Variants")]
    [SerializeField] private GameObject[] approachVariants;
    [SerializeField] private GameObject[] cameraVariants;

    [SerializeField] private int currentApproachIndex;
    [SerializeField] private int currentCameraIndex;

    public void SetApproachVariant(int index)
    {
        currentApproachIndex = Mathf.Clamp(index, 0, Mathf.Max(0, approachVariants.Length - 1));

        for (int i = 0; i < approachVariants.Length; i++)
        {
            if (approachVariants[i] != null)
            {
                approachVariants[i].SetActive(i == currentApproachIndex);
            }
        }
    }

    public void SetCameraVariant(int index)
    {
        currentCameraIndex = Mathf.Clamp(index, 0, Mathf.Max(0, cameraVariants.Length - 1));

        for (int i = 0; i < cameraVariants.Length; i++)
        {
            if (cameraVariants[i] != null)
            {
                cameraVariants[i].SetActive(i == currentCameraIndex);
            }
        }
    }

    [ContextMenu("Apply Current Variants")]
    public void ApplyCurrentVariants()
    {
        SetApproachVariant(currentApproachIndex);
        SetCameraVariant(currentCameraIndex);
    }
}
