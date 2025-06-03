using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerMovementStatsUI : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerMovementStats playerStats;
    public GameObject verticalLayoutGroup;

    [Header("Prefabs UI")]
    public GameObject floatSliderPrefab;
    public GameObject intSliderPrefab;
    public GameObject togglePrefab;
    public GameObject labelPrefab;

    [Header("Configuración")]
    public bool updateInRealTime = true;
    public float updateInterval = 0.1f;

    private Dictionary<string, Component> uiElements = new Dictionary<string, Component>();
    private Dictionary<string, FieldInfo> fieldInfos = new Dictionary<string, FieldInfo>();

    void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerMovementStats no está asignado!");
            return;
        }

        GenerateUI();

        if (updateInRealTime)
        {
            StartCoroutine(UpdateValuesCoroutine());
        }
    }

    void GenerateUI()
    {
        // Limpiar UI existente
        ClearUI();

        // Obtener todos los campos públicos del ScriptableObject
        FieldInfo[] fields = playerStats.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        string currentHeader = "";

        foreach (FieldInfo field in fields)
        {
            // Verificar si el campo tiene atributo Header
            HeaderAttribute headerAttr = field.GetCustomAttribute<HeaderAttribute>();
            if (headerAttr != null)
            {
                currentHeader = headerAttr.header;
                CreateHeaderLabel(currentHeader);
            }

            // Ignorar campos que no queremos mostrar
            if (ShouldIgnoreField(field))
                continue;

            CreateUIForField(field);
        }
    }

    bool ShouldIgnoreField(FieldInfo field)
    {
        // Ignorar propiedades calculadas (solo getter)
        if (field.Name == "gravity" || field.Name == "initialJumpVelocity" || field.Name == "adjustedJumpHeight")
            return true;

        // Ignorar campos del sistema Unity
        if (field.FieldType == typeof(HideInInspector))
            return true;

        return false;
    }

    void CreateHeaderLabel(string headerText)
    {
        if (labelPrefab == null) return;

        GameObject headerObj = Instantiate(labelPrefab, verticalLayoutGroup.transform);
        TextMeshProUGUI headerLabel = headerObj.GetComponent<TextMeshProUGUI>();

        if (headerLabel != null)
        {
            headerLabel.text = headerText;
            headerLabel.fontSize = 18;
            headerLabel.fontStyle = FontStyles.Bold;
            headerLabel.color = Color.yellow;
        }
        else
        {
            // Fallback para Text normal
            Text textComponent = headerObj.GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = headerText;
                textComponent.fontSize = 18;
                textComponent.fontStyle = FontStyle.Bold;
                textComponent.color = Color.yellow;
            }
        }
    }

    void CreateUIForField(FieldInfo field)
    {
        GameObject uiElement = null;

        if (field.FieldType == typeof(float))
        {
            uiElement = CreateFloatSlider(field);
        }
        else if (field.FieldType == typeof(int))
        {
            uiElement = CreateIntSlider(field);
        }
        else if (field.FieldType == typeof(bool))
        {
            uiElement = CreateToggle(field);
        }
        else if (field.FieldType == typeof(LayerMask))
        {
            uiElement = CreateLayerMaskField(field);
        }

        if (uiElement != null)
        {
            fieldInfos[field.Name] = field;
        }
    }

    GameObject CreateFloatSlider(FieldInfo field)
    {
        if (floatSliderPrefab == null) return null;

        GameObject sliderObj = Instantiate(floatSliderPrefab, verticalLayoutGroup.transform);
        Slider slider = sliderObj.GetComponent<Slider>();

        if (slider != null)
        {
            // Obtener valores del atributo Range si existe
            RangeAttribute rangeAttr = field.GetCustomAttribute<RangeAttribute>();
            if (rangeAttr != null)
            {
                slider.minValue = rangeAttr.min;
                slider.maxValue = rangeAttr.max;
            }
            else
            {
                // Valores por defecto
                slider.minValue = 0f;
                slider.maxValue = 100f;
            }

            // Establecer valor actual
            float currentValue = (float)field.GetValue(playerStats);
            slider.value = currentValue;

            // Configurar label
            SetupSliderLabel(sliderObj, field.Name, currentValue);

            // Añadir listener para actualizar el valor
            slider.onValueChanged.AddListener((value) =>
            {
                field.SetValue(playerStats, value);
                UpdateSliderLabel(sliderObj, field.Name, value);
            });

            uiElements[field.Name] = slider;
        }

        return sliderObj;
    }

    GameObject CreateIntSlider(FieldInfo field)
    {
        if (intSliderPrefab == null) return null;

        GameObject sliderObj = Instantiate(intSliderPrefab, verticalLayoutGroup.transform);
        Slider slider = sliderObj.GetComponent<Slider>();

        if (slider != null)
        {
            slider.wholeNumbers = true;

            // Obtener valores del atributo Range si existe
            RangeAttribute rangeAttr = field.GetCustomAttribute<RangeAttribute>();
            if (rangeAttr != null)
            {
                slider.minValue = rangeAttr.min;
                slider.maxValue = rangeAttr.max;
            }
            else
            {
                slider.minValue = 1;
                slider.maxValue = 10;
            }

            int currentValue = (int)field.GetValue(playerStats);
            slider.value = currentValue;

            SetupSliderLabel(sliderObj, field.Name, currentValue);

            slider.onValueChanged.AddListener((value) =>
            {
                int intValue = Mathf.RoundToInt(value);
                field.SetValue(playerStats, intValue);
                UpdateSliderLabel(sliderObj, field.Name, intValue);
            });

            uiElements[field.Name] = slider;
        }

        return sliderObj;
    }

    GameObject CreateToggle(FieldInfo field)
    {
        if (togglePrefab == null) return null;

        GameObject toggleObj = Instantiate(togglePrefab, verticalLayoutGroup.transform);
        Toggle toggle = toggleObj.GetComponent<Toggle>();

        if (toggle != null)
        {
            bool currentValue = (bool)field.GetValue(playerStats);
            toggle.isOn = currentValue;

            // Configurar label
            SetupToggleLabel(toggleObj, field.Name);

            toggle.onValueChanged.AddListener((value) =>
            {
                field.SetValue(playerStats, value);
            });

            uiElements[field.Name] = toggle;
        }

        return toggleObj;
    }

    GameObject CreateLayerMaskField(FieldInfo field)
    {
        // Para LayerMask, crear un label que muestre el valor actual
        if (labelPrefab == null) return null;

        GameObject labelObj = Instantiate(labelPrefab, verticalLayoutGroup.transform);
        TextMeshProUGUI label = labelObj.GetComponent<TextMeshProUGUI>();

        if (label != null)
        {
            LayerMask currentValue = (LayerMask)field.GetValue(playerStats);
            label.text = $"{field.Name}: {currentValue.value}";
        }
        else
        {
            Text textComponent = labelObj.GetComponent<Text>();
            if (textComponent != null)
            {
                LayerMask currentValue = (LayerMask)field.GetValue(playerStats);
                textComponent.text = $"{field.Name}: {currentValue.value}";
            }
        }

        uiElements[field.Name] = label ?? (Component)labelObj.GetComponent<Text>();
        return labelObj;
    }

    void SetupSliderLabel(GameObject sliderObj, string fieldName, float value)
    {
        TextMeshProUGUI[] labels = sliderObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (labels.Length > 0)
        {
            labels[0].text = $"{FormatFieldName(fieldName)}: {value:F2}";
        }
        else
        {
            Text[] textLabels = sliderObj.GetComponentsInChildren<Text>();
            if (textLabels.Length > 0)
            {
                textLabels[0].text = $"{FormatFieldName(fieldName)}: {value:F2}";
            }
        }
    }

    void UpdateSliderLabel(GameObject sliderObj, string fieldName, float value)
    {
        SetupSliderLabel(sliderObj, fieldName, value);
    }

    void SetupToggleLabel(GameObject toggleObj, string fieldName)
    {
        TextMeshProUGUI[] labels = toggleObj.GetComponentsInChildren<TextMeshProUGUI>();
        if (labels.Length > 0)
        {
            labels[0].text = FormatFieldName(fieldName);
        }
        else
        {
            Text[] textLabels = toggleObj.GetComponentsInChildren<Text>();
            if (textLabels.Length > 0)
            {
                textLabels[0].text = FormatFieldName(fieldName);
            }
        }
    }

    string FormatFieldName(string fieldName)
    {
        // Convertir camelCase a texto legible
        string result = "";
        for (int i = 0; i < fieldName.Length; i++)
        {
            if (i > 0 && char.IsUpper(fieldName[i]))
            {
                result += " ";
            }
            result += i == 0 ? char.ToUpper(fieldName[i]) : fieldName[i];
        }
        return result;
    }

    void ClearUI()
    {
        // Limpiar elementos UI existentes
        foreach (Transform child in verticalLayoutGroup.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        uiElements.Clear();
        fieldInfos.Clear();
    }

    IEnumerator UpdateValuesCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            UpdateUIValues();
        }
    }

    void UpdateUIValues()
    {
        foreach (var kvp in fieldInfos)
        {
            string fieldName = kvp.Key;
            FieldInfo field = kvp.Value;

            if (uiElements.ContainsKey(fieldName))
            {
                Component uiComponent = uiElements[fieldName];

                if (uiComponent is Slider slider)
                {
                    object currentValue = field.GetValue(playerStats);
                    if (currentValue is float floatValue)
                    {
                        if (Mathf.Abs(slider.value - floatValue) > 0.001f)
                        {
                            slider.value = floatValue;
                        }
                    }
                    else if (currentValue is int intValue)
                    {
                        if (Mathf.Abs(slider.value - intValue) > 0.001f)
                        {
                            slider.value = intValue;
                        }
                    }
                }
                else if (uiComponent is Toggle toggle)
                {
                    bool currentValue = (bool)field.GetValue(playerStats);
                    if (toggle.isOn != currentValue)
                    {
                        toggle.isOn = currentValue;
                    }
                }
            }
        }
    }

    // Método público para regenerar la UI manualmente
    [ContextMenu("Regenerate UI")]
    public void RegenerateUI()
    {
        GenerateUI();
    }
}