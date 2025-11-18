using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Ruler", false)]
public class RulerOverlay : Overlay
{
    private const string showDistancePath = "RulerTool.ShowDistance";
    private const string showDeltasPath = "RulerTool.ShowDeltas";
    private const string showSizePath = "RulerTool.ShowSize";
    private const string showBoundsPath = "RulerTool.ShowBounds";
    private const string fontSizePath = "RulerTool.FontSize";
    
    public static bool showDistance { get; private set; } = false;
    public static bool showDeltas { get; private set; } = false;
    public static bool showSize { get; private set; } = false;
    public static bool showBounds { get; private set; } = false;
    public static int fontSize { get; private set; } = 18;
    
    private Toggle showDistanceToggle;
    private Toggle showDeltasToggle;
    private Toggle showSizeToggle;
    private Toggle showBoundsToggle;
    private IntegerField fontSizeField;

    public override VisualElement CreatePanelContent()
    {
        showDistance = EditorPrefs.GetBool(showDistancePath, false);
        showDeltas = EditorPrefs.GetBool(showDeltasPath, false);
        showSize = EditorPrefs.GetBool(showSizePath, false);
        showBounds = EditorPrefs.GetBool(showBoundsPath, false); 
        fontSize = EditorPrefs.GetInt(fontSizePath, 18);
        
        var root = new VisualElement() { name = "root" };
        root.styleSheets.Add(Resources.Load<StyleSheet>("RulerStyles"));
        
        showDistanceToggle = new Toggle() { name = "distance-toggle", value = showDistance, text = "Show Distance" };
        root.Add(showDistanceToggle);
        showDistanceToggle.RegisterValueChangedCallback(ShowDistance_OnValueChanged);
        
        showDeltasToggle = new Toggle() { name = "deltas-toggle", value = showDeltas, text = "Show Deltas" };
        root.Add(showDeltasToggle);
        showDeltasToggle.RegisterValueChangedCallback(ShowDeltas_OnValueChanged);
        
        showSizeToggle = new Toggle() { name = "size-toggle", value = showSize, text = "Show Size" };
        root.Add(showSizeToggle);
        showSizeToggle.RegisterValueChangedCallback(ShowSize_OnValueChanged);
        
        showBoundsToggle = new Toggle() { name = "bounds-toggle", value = showBounds, text = "Show Bounds" };
        root.Add(showBoundsToggle);
        showBoundsToggle.RegisterValueChangedCallback(ShowBounds_OnValueChanged);

        var fontSizeLabel = new Label() { name = "font-size-label", text = "Font Size" };
        root.Add(fontSizeLabel);
        fontSizeField = new IntegerField() { name = "font-size", value = fontSize };
        fontSizeLabel.Add(fontSizeField);
        fontSizeField.RegisterValueChangedCallback(FontSize_OnValueChanged);
        
        return root;
    }

    private void FontSize_OnValueChanged(ChangeEvent<int> evt)
    {
        EditorPrefs.SetInt(fontSizePath, evt.newValue);
        fontSize = evt.newValue;
    }
    
    private void ShowBounds_OnValueChanged(ChangeEvent<bool> evt)
    {
        EditorPrefs.SetBool(showBoundsPath, evt.newValue);
        showBounds = evt.newValue;
    }

    private void ShowDeltas_OnValueChanged(ChangeEvent<bool> evt)
    {
        EditorPrefs.SetBool(showDeltasPath, evt.newValue);
        showDeltas = evt.newValue;
    }

    private void ShowSize_OnValueChanged(ChangeEvent<bool> evt)
    {
        EditorPrefs.SetBool(showSizePath, evt.newValue);
        showSize = evt.newValue;
    }

    private void ShowDistance_OnValueChanged(ChangeEvent<bool> evt)
    {
        EditorPrefs.SetBool(showDistancePath, evt.newValue);
        showDistance = evt.newValue;
    }

    public override void OnWillBeDestroyed()
    {
        base.OnWillBeDestroyed();
        showDistanceToggle.UnregisterValueChangedCallback(ShowDistance_OnValueChanged);
        showDeltasToggle.UnregisterValueChangedCallback(ShowDeltas_OnValueChanged);
        showSizeToggle.UnregisterValueChangedCallback(ShowSize_OnValueChanged);
        showBoundsToggle.UnregisterValueChangedCallback(ShowBounds_OnValueChanged);
        fontSizeField.UnregisterValueChangedCallback(FontSize_OnValueChanged);
    }
}


[EditorTool("Ruler Tool", typeof(GameObject))]
public class GameObjectRulerTool : EditorTool, IDrawSelectedHandles
{
    private const float lineSize = 4f;

    public void OnDrawHandles()
    {
         if (RulerOverlay.showDistance)
             DrawDistance();

         if (RulerOverlay.showDeltas)
             DrawDeltas();
         
         if (RulerOverlay.showSize)
            DrawSize();

         if (RulerOverlay.showBounds)
             DrawBounds();
    }

    private void DrawBounds()
    {
        Handles.color = Color.cyan;
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            var mr = Selection.gameObjects[i].GetComponent<MeshRenderer>();
            if (mr == null)
                continue;
            
            Handles.DrawWireCube(mr.bounds.center, mr.bounds.size);
        }
    }

    private void DrawDeltas()
    {
        Handles.color = Color.cyan;
        GUIStyle labelStyle = new GUIStyle()
        {
            fontSize = RulerOverlay.fontSize,
            normal = new GUIStyleState()
            {
                textColor = Color.cyan
            }
        };
        
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            for (int j = i + 1; j < Selection.gameObjects.Length; j++)
            {
                var from = Selection.gameObjects[i];
                var to = Selection.gameObjects[j];
                
                var fromMR = from.GetComponent<MeshRenderer>();
                var toMR = to.GetComponent<MeshRenderer>();
                if (fromMR != null && toMR != null)
                {
                    var p1 = fromMR.bounds.ClosestPoint(toMR.transform.position);
                    var p2 = toMR.bounds.ClosestPoint(fromMR.transform.position);
                    var b = new Bounds();
                    b.SetMinMax(p1, p2);
                    
                    Handles.color = Color.cyan;
                    Handles.DrawWireCube(b.center, b.size);

                    if (b.size.x != 0)
                    {
                        Handles.color = Color.red;
                        Handles.DrawDottedLine(
                            b.center - Vector3.right * b.size.x * 0.5f,
                            b.center + Vector3.right * b.size.x * 0.5f,
                            lineSize);

                        labelStyle.normal.textColor = Color.red;
                        Handles.Label(b.center + Vector3.right * b.size.x * 0.25f,
                            $"{(b.size.x):F2}",
                            labelStyle
                        );
                    }

                    if (b.size.y != 0)
                    {
                        Handles.color = Color.green;
                        Handles.DrawDottedLine(
                            b.center - Vector3.up * b.size.y * 0.5f,
                            b.center + Vector3.up * b.size.y * 0.5f,
                            lineSize);

                        labelStyle.normal.textColor = Color.green;
                        Handles.Label(b.center + Vector3.up * b.size.y * 0.25f,
                            $"{(b.size.y):F2}",
                            labelStyle
                        );
                    }

                    if (b.size.z != 0)
                    {
                        Handles.color = Color.blue;
                        Handles.DrawDottedLine(
                            b.center - Vector3.forward * b.size.z * 0.5f,
                            b.center + Vector3.forward * b.size.z * 0.5f,
                            lineSize);

                        labelStyle.normal.textColor = Color.blue;
                        Handles.Label(b.center + Vector3.forward * b.size.z * 0.25f,
                            $"{(b.size.z):F2}",
                            labelStyle
                        );
                    }
                }
                
            }
        }
    }

    private void DrawDistance()
    {
        Handles.color = Color.cyan;
        GUIStyle labelStyle = new GUIStyle()
        {
            fontSize = RulerOverlay.fontSize,
            normal = new GUIStyleState()
            {
                textColor = Color.cyan
            }
        };
        
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            for (int j = i + 1; j < Selection.gameObjects.Length; j++)
            {
                var from = Selection.gameObjects[i];
                var to = Selection.gameObjects[j];
                
                Handles.DrawDottedLine(
                    from.transform.position,
                    to.transform.position,
                    lineSize
                );
                
                Handles.Label(
                    (from.transform.position + to.transform.position) / 2.0f,
                    $"{Vector3.Distance(from.transform.position, to.transform.position):F2}",
                    labelStyle
                );
            }
        }
    }

    private void DrawSize()
    {
        foreach (var x in Selection.gameObjects)
        {
            var meshRenderer = x.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                GUIStyle labelStyle = new GUIStyle()
                {
                    fontSize = RulerOverlay.fontSize
                };
                Vector3 n = Vector3.zero;
                 
                Handles.color = Color.red;
                n = x.transform.right * meshRenderer.localBounds.extents.x * x.transform.localScale.x;
                Handles.DrawDottedLine(
                    x.transform.position - n,
                    x.transform.position + n,
                    lineSize
                );
                 
                labelStyle.normal.textColor = Color.red;
                Handles.Label(x.transform.position + (n / 2),
                    $"{(meshRenderer.localBounds.size.x * x.transform.localScale.x):F2}",
                    labelStyle
                );
                 
                Handles.color = Color.green;
                n = x.transform.up * meshRenderer.localBounds.extents.y * x.transform.localScale.y;
                Handles.DrawDottedLine(
                    x.transform.position - n,
                    x.transform.position + n,
                    lineSize);
                 
                labelStyle.normal.textColor = Color.green;
                Handles.Label(x.transform.position + (n / 2),
                    $"{(meshRenderer.localBounds.size.y * x.transform.localScale.y):F2}",
                    labelStyle
                );
                 
                Handles.color = Color.blue;
                n = x.transform.forward * meshRenderer.localBounds.extents.z * x.transform.localScale.z;
                Handles.DrawDottedLine(
                    x.transform.position - n,
                    x.transform.position + n,
                    lineSize
                );
                 
                labelStyle.normal.textColor = Color.blue;
                Handles.Label(x.transform.position + (n / 2),
                    $"{(meshRenderer.localBounds.size.z * x.transform.localScale.z):F2}",
                    labelStyle
                );
            }
        }
    }
}
