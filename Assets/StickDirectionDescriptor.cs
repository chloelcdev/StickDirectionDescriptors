using UnityEngine;

[System.Serializable]
public class StickDirectionDescriptor
{
    [Range(0f, 360f)]
    public float angle = 0f; // The direction of the center of the allowed area

    [Range(0f, 360f)]
    public float tolerance = 360f; // How much deviation from the angle is allowed

    [Range(0f, 1f)]
    public float deadzone = 0; // Minimum stick movement required (0 = no deadzone)

    public bool Matches(Vector2 input)//, bool debugInputAngle, bool debugTargetAngle, bool debugAngleDifference)
    {
        Vector2 normalizedInput = input.normalized;

        // Deadzone Check
        if (input.magnitude < deadzone)
            return false;

        // Find the angle of the input so we can compare our angle
        float inputAngle = Mathf.Rad2Deg * Mathf.Atan2(normalizedInput.y, normalizedInput.x);

        float angleDifference = Mathf.DeltaAngle(angle, inputAngle); // Angle difference, handling angle wraparound
        float degreesFromAngle = Mathf.Abs(angleDifference);

        float halfTolerance = tolerance / 2f; // half tolerance because we're looking at angle from center

        return degreesFromAngle <= halfTolerance;
    }
}