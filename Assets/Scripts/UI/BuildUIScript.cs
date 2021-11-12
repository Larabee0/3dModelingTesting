using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class BuildUIScript : MonoBehaviour
{
    // Buttons
    [SerializeField] private Button EditButton;
    [SerializeField] private Button ResetMeshButton;
    [SerializeField] private Button DeselectButton;
    [SerializeField] private Button DetailingButton;
    [SerializeField] private Button FlightModeButton;

    // Toggle Containers
    [SerializeField] private VisualElement SelectionToggle;
    [SerializeField] private VisualElement VertexSelectionToggle;
    [SerializeField] private VisualElement AddingModeToggle;
    [SerializeField] private VisualElement QuadSelectionToggle;

    // General Containers
    [SerializeField] private VisualElement LocationContainer;
    [SerializeField] private VisualElement RotationContainer;
    [SerializeField] private VisualElement ScaleContainer;
    [SerializeField] private VisualElement RoundingContainer;
    [SerializeField] private VisualElement CamLocationContainer;
    [SerializeField] private VisualElement Toolbar;


    // Public Fields

    // Toggle Indication values
    public bool MainSelection = false;
    public bool VertexSelection = true;
    public bool RoundingMode = false;
    public bool AddingMode = false;
    public bool QuadSelection = false;

    //Text Fields
    public TextField[] LocationTextFields = new TextField[3];
    public TextField[] RotationTextFields = new TextField[3];
    public TextField[] ScaleTextFields = new TextField[4];
    public TextField[] CameraTextFields = new TextField[3];
    public TextField RoundingTextField;

    public List<string> ButtonsToSpawn = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        EditButton = rootVisualElement.Q<Button>("EditButton");
        ResetMeshButton = rootVisualElement.Q<Button>("ResetMeshButton");
        DeselectButton = rootVisualElement.Q<Button>("DeselectButton");
        DetailingButton = rootVisualElement.Q<Button>("DetailingButton");
        FlightModeButton = rootVisualElement.Q<Button>("FlightModeButton");

        LocationTextFields[0] = rootVisualElement.Q<TextField>("LocationXaxis");
        LocationTextFields[1] = rootVisualElement.Q<TextField>("LocationYaxis");
        LocationTextFields[2] = rootVisualElement.Q<TextField>("LocationZaxis");
        
        RotationTextFields[0] = rootVisualElement.Q<TextField>("RotationXaxis");
        RotationTextFields[1] = rootVisualElement.Q<TextField>("RotationYaxis");
        RotationTextFields[2] = rootVisualElement.Q<TextField>("RotationZaxis");

        ScaleTextFields[0] = rootVisualElement.Q<TextField>("ScaleXaxis");
        ScaleTextFields[1] = rootVisualElement.Q<TextField>("ScaleYaxis");
        ScaleTextFields[2] = rootVisualElement.Q<TextField>("ScaleZaxis");
        ScaleTextFields[3] = rootVisualElement.Q<TextField>("ScaleXYZaxis");

        CameraTextFields[0] = rootVisualElement.Q<TextField>("CamLocationXaxis");
        CameraTextFields[1] = rootVisualElement.Q<TextField>("CamLocationYaxis");
        CameraTextFields[2] = rootVisualElement.Q<TextField>("CamLocationZaxis");

        SelectionToggle = rootVisualElement.Q<VisualElement>("SelectionModeElement");
        VertexSelectionToggle = rootVisualElement.Q<VisualElement>("VetexSelectionModeElement");
        AddingModeToggle = rootVisualElement.Q<VisualElement>("AddingModeElement");
        QuadSelectionToggle = rootVisualElement.Q<VisualElement>("QuadSelectionElement");

        LocationContainer = rootVisualElement.Q<VisualElement>("LocationContainer");
        RotationContainer = rootVisualElement.Q<VisualElement>("RotationContainer");
        ScaleContainer = rootVisualElement.Q<VisualElement>("ScaleContainer");
        RoundingContainer = rootVisualElement.Q<VisualElement>("RoundingContainer");
        CamLocationContainer = rootVisualElement.Q<VisualElement>("CameraContainer");

        Toolbar = rootVisualElement.Q<VisualElement>("Toolbar");

        
        RoundingTextField = RoundingContainer.Q<TextField>("DPs");


        EditButton.RegisterCallback<ClickEvent>(ev => EditButtonOnClick());
        ResetMeshButton.RegisterCallback<ClickEvent>(ev => ResetMeshButtonOnClick());
        DeselectButton.RegisterCallback<ClickEvent>(ev => DeselectButtonOnClick());
        DetailingButton.RegisterCallback<ClickEvent>(ev => DetailingButtonOnClick());
        FlightModeButton.RegisterCallback<ClickEvent>(ev => FlightModeButtonOnClick());

        RoundingContainer.Q<Toggle>("RoundingToggle").RegisterCallback<ClickEvent>(ev => RoundingToggleOnClick());
        SelectionToggle.Q<Toggle>("SelectionToggle").RegisterCallback<ClickEvent>(ev => SelectionToggleOnClick());
        VertexSelectionToggle.Q<Toggle>("VertexSelectionToggle").RegisterCallback<ClickEvent>(ev => VertexSelectionToggleOnClick());
        AddingModeToggle.Q<Toggle>("AddingModeToggle").RegisterCallback<ClickEvent>(ev => AddingModeToggleOnClick());
        QuadSelectionToggle.Q<Toggle>("QuadSelectionToggle").RegisterCallback<ClickEvent>(ev => QuadSelectionToggleOnClick());


        SpawnToolbarButtons();
    }

    void SpawnToolbarButtons()
    {
        for (int i = 0; i < ButtonsToSpawn.Count; i++)
        {
            Toolbar.Add(new Button());
            Button Temp = Toolbar.Q<Button>("");
            Temp.name = i.ToString();
            Temp.style.flexDirection = FlexDirection.Column;
            Temp.style.justifyContent = Justify.FlexEnd;
            Temp.text = ButtonsToSpawn[i];
            Temp.style.unityTextAlign = TextAnchor.MiddleCenter;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void EditButtonOnClick()
    {
        Debug.Log("Edit button clicked");
        
        if (ResetMeshButton.style.display == DisplayStyle.Flex)
        {
            ResetMeshButton.style.display = DisplayStyle.None;
            RoundingContainer.style.display = DisplayStyle.None;            
            VertexSelectionToggle.style.display = DisplayStyle.None;
            AddingModeToggle.style.display = DisplayStyle.None;

            SelectionToggle.style.display = DisplayStyle.Flex;
            RotationContainer.style.display = DisplayStyle.Flex;
            ScaleContainer.style.display = DisplayStyle.Flex;


            QuadSelectionToggle.style.display = DisplayStyle.None;
            AddingModeToggle.Q<Toggle>("AddingModeToggle").value = false;
            QuadSelectionToggle.Q<Toggle>("QuadSelectionToggle").value = false;
        }
        else if(SelectionToggle.Q<Toggle>("SelectionToggle").value)
        {
            ResetMeshButton.style.display = DisplayStyle.Flex;
            RoundingContainer.style.display = DisplayStyle.Flex;
            VertexSelectionToggle.style.display = DisplayStyle.Flex;
            AddingModeToggle.style.display = DisplayStyle.Flex;

            SelectionToggle.style.display = DisplayStyle.None;
            RotationContainer.style.display = DisplayStyle.None;
            ScaleContainer.style.display = DisplayStyle.None;
            if (!VertexSelectionToggle.Q<Toggle>("VertexSelectionToggle").value)
            {
                VertexSelectionToggleOnClick();
            }
        }
    }

    void ResetMeshButtonOnClick()
    {
        Debug.LogWarning("Reset mesh not implemented!");
    }

    void DeselectButtonOnClick()
    {
        if (ResetMeshButton.style.display == DisplayStyle.Flex)
        {
            EditButtonOnClick();
        }
        //if (!SelectionToggle.Q<Toggle>("SelectionToggle").value)
        //{
        //    SelectionToggle.Q<Toggle>("SelectionToggle").value = true;
        //}
    }

    void DetailingButtonOnClick()
    {
        Debug.LogWarning("Detailling mode not implemented!");
    }

    void FlightModeButtonOnClick()
    {
        Debug.LogWarning("Flight mode not implemented!");
    }

    void RoundingToggleOnClick()
    {
        if (RoundingContainer.Q<Toggle>("RoundingToggle").value)
        {
            Debug.Log("Rounding Enabled");
            RoundingMode = true;
        }
        else
        {
            RoundingMode = false;
            Debug.Log("Rounding Disabled");
        }
    }

    void SelectionToggleOnClick()
    {
        if (SelectionToggle.Q<Toggle>("SelectionToggle").value)
        {
            MainSelection = true;
        }
        else
        {
            MainSelection = false;
        }
    }

    void VertexSelectionToggleOnClick()
    {
        if (VertexSelectionToggle.Q<Toggle>("VertexSelectionToggle").value)
        {
            VertexSelection = true;
        }
        else
        {
            VertexSelection = false;
        }
    }

    void AddingModeToggleOnClick()
    {
        if (AddingModeToggle.Q<Toggle>("AddingModeToggle").value)
        {
            AddingMode = true;
            QuadSelectionToggle.style.display = DisplayStyle.Flex;
        }
        else
        {
            AddingMode = false;
            QuadSelectionToggle.style.display = DisplayStyle.None;
        }
    }

    void QuadSelectionToggleOnClick()
    {
        if (QuadSelectionToggle.Q<Toggle>("QuadSelectionToggle").value)
        {
            QuadSelection = true;
        }
        else
        {
            QuadSelection = false;
        }
    }
}
