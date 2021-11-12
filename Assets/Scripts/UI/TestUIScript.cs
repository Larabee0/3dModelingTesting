using System;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

[RequireComponent(typeof(UIDocument))]
public class TestUIScript : MonoBehaviour
{
    [SerializeField] Button Button;
    [SerializeField] Label Lable;
    [SerializeField] TextField TextField1;
    public int EventCounter = 0;

    private void Start()
    {
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        Button = rootVisualElement.Q<Button>("Button");
        Lable = rootVisualElement.Q<Label>("Lable1");
        TextField1 = rootVisualElement.Q<TextField>("TextField1");


        TextField1.RegisterCallback<FocusOutEvent>(ev => OnTextFieldChange());
        Button.RegisterCallback<ClickEvent>(ev => OnButtonClick());
    }

    void OnButtonClick()
    {
        EventCounter += 1;
        //Debug.Log("Button clicked");
    }

    void OnTextFieldChange()
    {
        Debug.Log(TextField1.text);
    }

    private void Update()
    {
        Lable.text = EventCounter.ToString();
    }
}
