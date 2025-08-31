using System;
//author: Mark Guo
[Serializable]
public struct Task
{
    public string name;
    public string description;

    public Task(string name, string description)
    {
        this.name = name;
        this.description = description;
    }

    public string getName() => name;
    public string getDescription() => description;
    public void setName(string name) => this.name = name;
    public void setDescription(string description) => this.description = description;
}
