using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Serialization;
public class StepEditorController : MonoBehaviour
{
    [FormerlySerializedAs("stepEditorUXML")]
    [Tooltip("The UXML document linked to the UIDocument component.")]
    [SerializeField] private VisualTreeAsset stepEditorUxml;
    
    [FormerlySerializedAs("stepListItemUXML")]
    [Tooltip("The UXML template for a single list item.")]
    [SerializeField] private VisualTreeAsset stepListItemUxml;

    // --- Private UI Element References ---
    private ListView _stepListView;
    private Button _addStepButton;

    // --- Data Model ---
    private List<StepData> _stepSequence = new List<StepData>();

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;

        // Query and hook up list view and add step button
        _stepListView = root.Q<ListView>("step-list");
        _addStepButton = root.Q<Button>("add-step-button");

        // Load initial data
        InitializeStepData();
        
        // Set up the list view's content
        SetupListView();
        
        // Register callback
        _addStepButton.clicked += OnAddStepClicked;
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
            visualElement.Q<Label>("action-label").text = data.ActionName;
            visualElement.Q<TextField>("value-field").value = data.Value.ToString("F1");
            Button removeButton = visualElement.Q<Button>("remove-button");
            removeButton.clicked += () => OnRemoveStepClicked(index);
        };
        _stepListView.itemIndexChanged += OnStepSequenceReordered;
    }

    private void OnAddStepClicked()
    {
        _stepSequence.Add(new StepData("New Action", 0.0f));
        _stepListView.itemsSource = _stepSequence; // Re-setting the source often triggers a refresh
        _stepListView.RefreshItems();
        _stepListView.ScrollToItem(_stepSequence.Count - 1);
    }

    private void OnRemoveStepClicked(int index)
    {
        if (index >= 0 && index < _stepSequence.Count)
        {
            _stepSequence.RemoveAt(index);
        }
        
        _stepListView.itemsSource = _stepSequence;
        _stepListView.RefreshItems();
    }
    
    private void OnStepSequenceReordered(int oldIndex, int newIndex)
    {
        Debug.Log($"Step sequence reordered: {_stepSequence[newIndex].ActionName} moved from index {oldIndex} to {newIndex}");
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
    }
}