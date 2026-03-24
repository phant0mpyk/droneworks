[System.Serializable]
public class PID
{
    public float Kp;
    public float Ki;
    public float Kd;

    float integral;
    float lastError;

    public float GetValue(float error, float dt)
    {
        integral += error * dt;
        float derivative = (error - lastError) / dt;
        lastError = error;
        return Kp * error + Ki * integral + Kd * derivative;
    }
}