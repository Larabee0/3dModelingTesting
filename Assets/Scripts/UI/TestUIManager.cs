using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TestUIManager : VisualElement
{
    VisualElement m_TitleScreen;

    string m_SceneName = "Main";

    //public new class UxmelFactory : UxmelFactory<TestUIManager, UxmlTraits> { }

    public new class uxmlTraits : VisualElement.UxmlTraits
    {
        UxmlStringAttributeDescription m_StartScene = new UxmlStringAttributeDescription { name = "start-scene", defaultValue = "Main" };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var sceneName = m_StartScene.GetValueFromBag(bag, cc);
            ((TestUIManager)ve).Init(sceneName);
        }

        
    }

    public TestUIManager()
    {
        this.RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
    }

    void OnGeometryChange(GeometryChangedEvent evt)
    {
        m_TitleScreen = this.Q("TitleScreen");
        this.UnregisterCallback<GeometryChangedEvent>(OnGeometryChange);
    }

    void Init(string sceneName)
    {
        m_SceneName = sceneName;
    }
}
