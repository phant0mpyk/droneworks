[System.Serializable]
public class PID
{
    public float Kp;
    public float Ki;
    public float Kd;

    float integral;
    float lastError;

    public float GetValue(float _error, float _deltaTime)
    {
        integral += _error * _deltaTime;
        float derivative = (_error - lastError) / _deltaTime;
        lastError = _error;
        return Kp * _error + Ki * integral + Kd * derivative;
    }
}