using System;
using System.Threading.Tasks;
using Tapper;
using TypedSignalR.Client;

namespace App.Interfaces.WhiteBoard;

[TranspilationSource]
enum ShapeType
{
    Circle,
    Box,
}

[Hub]
internal interface IWhiteboardHub
{
    Task<Guid> Add(ShapeType shapeType, float x, float y);
    Task Move(Guid shapeId);
    Task WriteLine(float x, float y);
}

[Receiver]
internal interface IWhiteboardReceiver
{
    Task OnMove(Guid shapeId, float x, float y);
    Task OnWriteLine(float x, float y);
}
