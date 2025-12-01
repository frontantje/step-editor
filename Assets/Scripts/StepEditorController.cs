using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
public class StepEditorController : MonoBehaviour
{
    [Tooltip("The UXML template for a single list item.")]
    [SerializeField] private VisualTreeAsset stepListItemUxml;
    
    [System.Serializable]
    private class StepDataWrapper
    {
        public List<StepData> steps; 

        public StepDataWrapper(List<StepData> s) 
        { 
            steps = s; 
        }
    }

    // Private UI Element References 
    private ListView _stepListView;
    private Button _addStepButton;

    // Data Model
    private List<StepData> _stepSequence;
    
    // Data Persistency
    private const string SaveFileName = "StepSequence.json";
    private string _savePath;

    private void Awake()
    {
        _stepSequence = new List<StepData>();
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;
        
        _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);

        // Query and hook up list view and add step button
        _stepListView = root.Q<ListView>("step-list");
        _addStepButton = root.Q<Button>("add-step-button");

        // Load initial data
        LoadStepSequence();;
        
        // Set up the list view's content
        SetupListView();
        
        // Register callback
        _addStepButton.clicked += OnAddStepClicked;
    }
    
    private void LoadStepSequence()
    {
        if (File.Exists(_savePath))
        {
            try
            {
                string json = File.ReadAllText(_savePath);
                StepDataWrapper wrapper = JsonUtility.FromJson<StepDataWrapper>(json);
            
                if (wrapper is { steps: not null })
                {
                    _stepSequence = wrapper.steps;
                    Debug.Log($"Loaded {_stepSequence.Count} steps from {_savePath}");
                    return; 
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading step sequence: {e.Message}. Starting with default data.");
            }
        }
        
        _stepSequence.Clear(); 
        InitializeStepData();
    }    
    private void SaveStepSequence()
    {
        StepDataWrapper wrapper = new StepDataWrapper(_stepSequence);
        string json = JsonUtility.ToJson(wrapper, true);
    
        try
        {
            File.WriteAllText(_savePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving step sequence: {e.Message}");
        }
    }

    private void InitializeStepData()
    {
        // Add a few initial steps to start the editor with content
        _stepSequence.Add(new StepData("Initial Move", 10.0f));
        _stepSequence.Add(new StepData("Wait", 2.5f));
        _stepSequence.Add(new StepData("Rotate", 90.0f));
    }

    private void SetupListView()
    {
        _stepListView.itemsSource = _stepSequence;
        _stepListView.makeItem = () =>
        {
            // Instantiate template
            VisualElement itemRoot = stepListItemUxml.Instantiate();
            Button removeButton = itemRoot.Q<Button>("remove-button");
            removeButton.clicked += () => OnRemoveButtonClicked(itemRoot);
            return itemRoot;
        };

        // Populate with data
        _stepListView.bindItem = (visualElement, index) =>
        {
            StepData data = _stepSequence[index];
            TextField valueField = visualElement.Q<TextField>("value-field");
            TextField titleField = visualElement.Q<TextField>("action-title");
    
            // 1. Data binding (clean)
            titleField.value = data.ActionName;
            valueField.value = data.Value.ToString("F1");
    
            // 2. Register/Unregister Value Changed (Necessary for data input)
            titleField.UnregisterValueChangedCallback(OnFieldValueChanged);
            valueField.UnregisterValueChangedCallback(OnFieldValueChanged);
            titleField.RegisterValueChangedCallback(OnFieldValueChanged);
            valueField.RegisterValueChangedCallback(OnFieldValueChanged);

            // 3. Store data model (Necessary for retrieving data in OnRemoveButtonClicked and OnFieldValueChanged)
            visualElement.userData = data;
        };
        _stepListView.itemIndexChanged += OnStepSequenceReordered;
    }

    private void OnRemoveButtonClicked(VisualElement itemRoot)
    {
        // Retrieve the stored StepData object
        if (itemRoot.userData is not StepData dataToRemove) return;
        // Find the current index of that specific object within the sequence list.
        int currentIndex = _stepSequence.IndexOf(dataToRemove);
        
        if (currentIndex != -1)
        {
            OnRemoveStepClicked(currentIndex);
        }
        else
        {
            Debug.LogError("Could not find data object in sequence list for removal.");
        }
    }

    private void OnFieldValueChanged(ChangeEvent<string> evt)
    {
        if (evt.target is TextField changedField)
        {
            object retrievedData = changedField.FindAncestorUserData(); 
            if (retrievedData is StepData data)
            {
                if (changedField.name == "value-field")
                {
                    if (float.TryParse(evt.newValue, out float newValue))
                    {
                        data.Value = newValue; 
                    }
                    else
                    {
                        return;
                    }
                }
                else if (changedField.name == "action-title") 
                {
                    data.ActionName = evt.newValue;
                }
            }
            SaveStepSequence();   
        }
    }
    private void OnAddStepClicked()
    {
        _stepSequence.Add(new StepData("New Action", 0.0f));
        _stepListView.itemsSource = _stepSequence; // Re-setting the source often triggers a refresh
        _stepListView.RefreshItems();
        _stepListView.ScrollToItem(_stepSequence.Count - 1);
        SaveStepSequence();
    }

    private void OnRemoveStepClicked(int index)
    {
        if (index >= 0 && index < _stepSequence.Count)
        {
            _stepSequence.RemoveAt(index);
        }
        
        _stepListView.itemsSource = _stepSequence;
        _stepListView.RefreshItems();
        SaveStepSequence();
    }
    
    private void OnStepSequenceReordered(int oldIndex, int newIndex)
    {
        Debug.Log($"Step sequence reordered: {_stepSequence[newIndex].ActionName} moved from index {oldIndex} to {newIndex}");
        SaveStepSequence();
    }

    private void OnDisable()
    {
        if (_addStepButton != null)
        {
            _addStepButton.clicked -= OnAddStepClicked;
        }
        if (_stepListView != null)
        {
            _stepListView.itemIndexChanged -= OnStepSequenceReordered;
        }
        SaveStepSequence();
    }
}