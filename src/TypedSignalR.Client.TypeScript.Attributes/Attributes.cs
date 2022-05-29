using System;

namespace TypedSignalR.Client.TypeScript;

[AttributeUsage(AttributeTargets.Interface)]
public class HubAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Interface)]
public class ReceiverAttribute : Attribute
{
}