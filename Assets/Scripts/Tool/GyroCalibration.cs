using UnityEngine;

public static class GyroCalibration
{
    private static Vector3 baseAcceleration = Vector3.zero;
    private static bool isCalibrated = false;

    public static void Calibrate()
    {
        baseAcceleration = Input.acceleration;
        isCalibrated = true;
        Debug.Log($"[GyroCalibration] Zeroed gyroscope base acceleration to current handset posture: {baseAcceleration}");
    }

    public static Vector3 GetCalibratedAcceleration()
    {
        if (PlayerPrefs.GetInt("GyroEnabled", 1) == 0)
        {
            return Vector3.zero;
        }

        if (!isCalibrated)
        {
            Calibrate();
        }
        return Input.acceleration - baseAcceleration;
    }
}
