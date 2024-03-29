namespace TypedSignalR.Client.TypeScript.Tests.Shared;

[Hub]
public interface IClientResultsTestHub
{
    Task<bool> StartTest();
}

[Receiver]
public interface IClientResultsTestHubReceiver
{
    Task<Guid> GetGuidFromClient(); // struct
    Task<Person> GetPersonFromClient(); // user defined type
    Task<int> SumInClient(int left, int right); // calc
}
