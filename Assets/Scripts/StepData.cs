[System.Serializable]
public class StepData
{
    public string ActionName;
    public float Value;

    public StepData(string name, float val)
    {
        ActionName = name;
        Value = val;
    }

    public override string ToString() => $"{ActionName}: {Value}";
}