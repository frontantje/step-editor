using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Serialization;
public class StepEditorController : MonoBehaviour
{
    [FormerlySerializedAs("stepEditorUXML")]
    [Tooltip("The UXML document linked to the UIDocument component.")]
    [SerializeField] private VisualTreeAsset stepEditorUxml;
    
    [FormerlySerializedAs("stepListItemUXML")]
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
    private List<StepData> _stepSequence = new();
    
    // Data Persistency
    private const string SaveFileName = "StepSequence.json";
    private string _savePath;

    private void OnEnable()
    {
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
            Debug.Log($"Saved {_stepSequence.Count} steps to {_savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving step sequence: {e.Message}");
        }
    }

    private void InitializeStepData()
    {
        Debug.Log("Loading fallback data");
        // Add a few initial steps to start the editor with content
        _stepSequence.Add(new StepData("Initial Move", 10.0f));
        _stepSequence.Add(new StepData("Wait", 2.5f));
        _stepSequence.Add(new StepData("Rotate", 90.0f));
    }

    private void SetupListView()
    {
        _stepListView.itemsSource = _stepSequence;
        _stepListView.reorderable = true;
        _stepListView.fixedItemHeight = 40; // Matches the height set in USS for .step-item
        _stepListView.makeItem = () =>
        {
            // Instantiate template
            VisualElement itemRoot = stepListItemUxml.Instantiate();
            return itemRoot;
        };

        // Populate with data
        _stepListView.bindItem = (visualElement, index) =>
        {
            StepData data = _stepSequence[index];
            TextField valueField = visualElement.Q<TextField>("value-field");
            Button removeButton = visualElement.Q<Button>("remove-button");
    
            // Wire up UI elements
            visualElement.Q<Label>("action-label").text = data.ActionName;
            valueField.value = data.Value.ToString("F1");
    
            // Register/Unregister Value Changed
            valueField.UnregisterValueChangedCallback(OnValueFieldChanged);
            valueField.RegisterValueChangedCallback(OnValueFieldChanged);
    
            removeButton.clicked -= () => OnRemoveButtonClicked(visualElement); 
            removeButton.clicked += () => OnRemoveButtonClicked(visualElement);
    
            // Store data model
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

    private void OnValueFieldChanged(ChangeEvent<string> evt)
    {
        if (evt.target is TextField changedField)
        {
            VisualElement itemRoot = changedField.parent.parent; 
            // Retrieve data model
            StepData data = itemRoot.userData as StepData;
            if (data == null) return;
            // Parse and update
            if (float.TryParse(evt.newValue, out float newValue))
            {
                data.Value = newValue; 
                SaveStepSequence();   
            }
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