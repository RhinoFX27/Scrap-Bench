using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RevolverMenuController : MonoBehaviour
{
    [Header("Cylinder")]
    [SerializeField] private RectTransform cylinder;
    [SerializeField] private float rotationDuration = 0.15f;

    [Header("Labels")]
    [SerializeField] private TMP_Text previousLabel;
    [SerializeField] private TMP_Text selectedLabel;
    [SerializeField] private TMP_Text nextLabel;

    [Header("Actions")]
    [SerializeField]
    private string[] actions =
    {
        "Aim",
        "Brace",
        "Patch",
        "Quickdraw",
        "Suppress",
        "Load"
    };

    [Header("Combat")]
    [SerializeField] private CombatManager combatManager;

    private int selectedIndex;
    private bool isRotating;

    private const float DegreesPerChamber = 60f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateLabels();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRotating)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.W))
        {
            SelectPrevious();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) ||
                 Input.GetKeyDown(KeyCode.S))
        {
            SelectNext();
        }
        else if (Input.GetKeyDown(KeyCode.Return) ||
                 Input.GetKeyDown(KeyCode.KeypadEnter) ||
                 Input.GetKeyDown(KeyCode.Z) ||
                 Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
        }
    }

    void SelectPrevious()
    {
        selectedIndex--;
        if (selectedIndex < 0)
            selectedIndex = actions.Length - 1;
        UpdateLabels();
        StartCoroutine(RotateCylinder(DegreesPerChamber));
    }

    void SelectNext()
    {
        selectedIndex++;
        if (selectedIndex >= actions.Length)
            selectedIndex = 0;
        UpdateLabels();
        StartCoroutine(RotateCylinder(-DegreesPerChamber));
    }

    void ConfirmSelection()
    {
        if (isRotating)
            return;
        string chosenAction = actions[selectedIndex];
        Debug.Log("Player selected: " + chosenAction);
        combatManager.PerformPlayerAction(chosenAction);
    }

    void UpdateLabels()
    {
        int previousIndex = WrapIndex(selectedIndex - 1);
        int nextIndex = WrapIndex(selectedIndex + 1);

        previousLabel.text = actions[previousIndex].ToUpper();
        selectedLabel.text = actions[selectedIndex].ToUpper();
        nextLabel.text = actions[nextIndex].ToUpper();
    }

    int WrapIndex(int index)
    {
        if (index < 0)
            return actions.Length - 1;
        if (index >= actions.Length)
            return 0;

        return index;
    }

    private IEnumerator RotateCylinder(float rotationAmount)
    {
        isRotating = true;
        Quaternion startRotation = cylinder.localRotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, 0f, rotationAmount);

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;

            float percentage = Mathf.Clamp01(elapsed / rotationDuration);

            float smoothedPercentage = Mathf.SmoothStep(0f, 1f, percentage);

            cylinder.localRotation = Quaternion.Lerp(startRotation, targetRotation, smoothedPercentage);

            yield return null;
        }
        cylinder.localRotation = targetRotation;
        isRotating = false;
    }
}
