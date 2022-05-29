import { ShapeType } from '../App.Interfaces.WhiteBoard';

export type IWhiteboardHub = {
    add(shapeType: ShapeType, x: number, y: number): Promise<string>;
    move(shapeId: string): Promise<void>;
    writeLine(x: number, y: number): Promise<void>;
}

export type IWhiteboardReceiver = {
    onMove(shapeId: string, x: number, y: number): Promise<void>;
    onWriteLine(x: number, y: number): Promise<void>;
}

