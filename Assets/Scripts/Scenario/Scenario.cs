using System.Collections.Generic;

[System.Serializable]
public class Scenario
{
    public string id;
    public string customer_type;
}

[System.Serializable]
public class ScenarioWrapper
{
    public List<Scenario> scenarios;
}