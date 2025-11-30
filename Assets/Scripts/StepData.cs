[System.Serializable]
public class StepData
{
    public string ActionName { get; set; }
    public float Value { get; set; }

    public StepData(string name, float val)
    {
        ActionName = name;
        Value = val;
    }

    public override string ToString() => $"{ActionName}: {Value}";
}