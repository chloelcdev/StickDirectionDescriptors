using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(StickDirectionDescriptor))]
public class StickDirectionDrawer : PropertyDrawer
{
    static float CircleRadius = 20f;

    static float propertySpacing = 4f;

    static float topMargin = 10f;
    static float bottomMargin = 6f;

    static float circleOutlineSize = 3f;

    static Color circleBackgroundColor = Color.Lerp(Color.black, Color.gray, 0.5f); // lerp color to not affect alpha


    bool isDragging = false;


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Initial height for the circle 
        float height = topMargin;

        height += ((CircleRadius + circleOutlineSize) * 2) + propertySpacing;

        // Add additional heights for each nested property
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("angle")) + propertySpacing;
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("tolerance")) + propertySpacing;
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("deadzone")) + propertySpacing;

        height += bottomMargin;

        return height;
    }


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Start a rect we'll mostly use for the rest of the time to track y position.
        var propRect = EditorGUI.PrefixLabel(position, label);

        float circleSizeWithOutline = (CircleRadius + circleOutlineSize) * 2;
        var circleRect = new Rect(propRect.xMin, propRect.yMin + propertySpacing + topMargin, circleSizeWithOutline, circleSizeWithOutline);


        // Get property values
        var angle = property.FindPropertyRelative("angle").floatValue;
        var tolerance = property.FindPropertyRelative("tolerance").floatValue;
        var deadzone = property.FindPropertyRelative("deadzone").floatValue;


        // Draw the background and outline of the circle
        DrawBackground(circleRect, circleBackgroundColor, Color.black);

        // Draw the green arc representing acceptable range
        DrawMainArc(circleRect, angle, tolerance, Color.green, deadzone);
        propRect.yMin = circleRect.yMax + propertySpacing;

        // just draws the basic float properties and [range] sliders (yum...)
        DrawBasicProperties(propRect, property);

        // Handles mouse input for easy direction setting in the circle
        CheckMouseInput(circleRect, property);

        EditorGUI.EndProperty();
    }


    void DrawBasicProperties(Rect propRect, SerializedProperty property)
    {
        propRect.yMax = propRect.yMin + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("angle")) + propertySpacing;

        EditorGUI.PropertyField(propRect, property.FindPropertyRelative("angle"), new GUIContent("Angle"));

        propRect.yMin += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("angle")) + propertySpacing;
        propRect.yMax = propRect.yMin + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("tolerance")) + propertySpacing;

        EditorGUI.PropertyField(propRect, property.FindPropertyRelative("tolerance"), new GUIContent("Tolerance"));

        propRect.yMin += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("tolerance")) + propertySpacing;
        propRect.yMax = propRect.yMin + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("deadzone")) + propertySpacing;

        EditorGUI.PropertyField(propRect, property.FindPropertyRelative("deadzone"), new GUIContent("Deadzone"));

    }

    /// <summary>
    /// Checks for mouse input on the circles for easy angle selection
    /// </summary>
    /// <param name="circleRect"></param>
    /// <param name="property"></param>
    void CheckMouseInput(Rect circleRect, SerializedProperty property)
    {
        // Click/Drag Detection 
        Event currentEvent = Event.current;

        if (currentEvent.button == 0)
        {
            if (currentEvent.type == EventType.MouseDown)
            {
                if (circleRect.Contains(currentEvent.mousePosition))
                    isDragging = true;
            }
            else if (currentEvent.type == EventType.MouseUp)
            {
                isDragging = false;
            }
            else if (currentEvent.type == EventType.MouseDrag && isDragging)
            {
                // Calculate new angle based on the click position
                Vector2 clickDirection = currentEvent.mousePosition - circleRect.center;

                // click y is reversed because +Y is down and -Y is up, which is the reverse of input systems
                float newAngle = Mathf.Atan2(-clickDirection.y, clickDirection.x) * Mathf.Rad2Deg;

                newAngle = (newAngle + 360) % 360; // Ensure angle is positive (0 to 360) (Atan2 range is -180 to 180)

                property.FindPropertyRelative("angle").floatValue = newAngle;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }

    private void DrawBackground(Rect rect, Color centerColor, Color outlineColor)
    {
        // draw the outline slightly bigger than the background
        Handles.color = outlineColor;
        Handles.DrawSolidArc(rect.center, Vector3.forward, Vector3.up, 360, CircleRadius + circleOutlineSize);

        // draw the main background
        Handles.color = centerColor;
        Handles.DrawSolidArc(rect.center, Vector3.forward, Vector3.up, 360, CircleRadius);
    }

    private void DrawMainArc(Rect rect, float angle, float coneAngle, Color color, float centerCoverage) 
    {
        var halfTolerance = coneAngle / 2f;
        var radius = CircleRadius; 

        var center = rect.center;
        var startDirection = Quaternion.AngleAxis(angle - halfTolerance, Vector3.back) * Vector3.right;
        var endDirection = Quaternion.AngleAxis(angle + halfTolerance, Vector3.back) * Vector3.right;

        // Calculate arc angle from direction vectors
        var arcAngle = Vector3.SignedAngle(startDirection, endDirection, Vector3.back);

        // Draw the arcs dark far edge (which is the actual full arc, just dark), lerp color to not affect alpha
        Handles.color = Color.Lerp(Color.black, color, 0.8f);
        Handles.DrawSolidArc(center, Vector3.back, startDirection, arcAngle, radius);

        // Draw the arcs bright majority
        Handles.color = color;
        Handles.DrawSolidArc(center, Vector3.back, startDirection, arcAngle, radius-2);

        // Draw the center coverage, which acts as our deadzone
        Handles.color = circleBackgroundColor;
        Handles.DrawSolidArc(center, Vector3.back, startDirection, 360, radius * centerCoverage);
    }

}