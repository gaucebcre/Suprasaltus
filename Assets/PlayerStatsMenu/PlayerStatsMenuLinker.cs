using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Reflection;
using System.Collections.Generic;

public class PlayerStatsMenuLinker : MonoBehaviour
{
    [SerializeField] private PlayerMovementStats stats;

    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private List<PlayerMovementStats> statsList;
    [SerializeField] private PlayerStatsMenuLinker linker;
    [SerializeField] private PlayerMovement playerMovement;


    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>(); // only 1 player
        SetupDropdown();
        // SetValues(); // done on OnDropdownChanged
    }

    void SetupDropdown()
    {
        // https://docs.unity3d.com/2018.4/Documentation/ScriptReference/UI.Dropdown-onValueChanged.html
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
        dropdown.ClearOptions();
        var dropdownOptions = new List<string>();
        foreach (var stats in statsList)
            dropdownOptions.Add(stats.statsName);
        dropdown.AddOptions(dropdownOptions);

        OnDropdownChanged(dropdown.value);
    }

    void OnDropdownChanged(int index)
    {
        stats = statsList[index];
        playerMovement.playerMoveStats = stats; // give player the selected stats
        SetValues();
    }

    void SetValues()
    {
        // must place in parent of sliders
        Slider[] sliders = GetComponentsInChildren<Slider>(true);

        foreach (Slider slider in sliders)
        {
            // The GameObject name must match the ScriptableObject field name !!!!!!!
            string fieldName = slider.gameObject.name;

            // https://learn.microsoft.com/en-us/dotnet/api/system.reflection.fieldinfo.getvalue
            var specificStat = typeof(PlayerMovementStats).GetField(fieldName); // var in case it's float
            // initial values
            slider.value = ConvertToFloat(specificStat.GetValue(stats));
            Transform label = slider.transform.Find("NumberLabel");
            Text nameLabel = label.GetComponent<Text>(); // Text, not TMP_Text 
            nameLabel.text = slider.value.ToString("0.##");

            slider.onValueChanged.RemoveAllListeners();

            // listeners for update
            slider.onValueChanged.AddListener((v) =>
            {
                specificStat.SetValue(stats, ConvertToFieldType(v, specificStat.FieldType));
                nameLabel.text = v.ToString("0.##");
            });
        }
    }

    float ConvertToFloat(object value)
    {
        if (value is float f) return f;
        if (value is int i) return i;
        if (value is double d) return (float)d;
        return 0f;
    }

    object ConvertToFieldType(float value, System.Type type)
    {
        if (type == typeof(float)) return value;
        if (type == typeof(int)) return Mathf.RoundToInt(value);
        if (type == typeof(double)) return (double)value;
        return value;
    }
}