import { ShapeType } from '../App.Interfaces.WhiteBoard';

export type IWhiteboardHub = {
    /**
    * @param shapeType Transpied from App.Interfaces.WhiteBoard.ShapeType
    * @param x Transpied from float
    * @param y Transpied from float
    * @returns Transpied from System.Threading.Tasks.Task<System.Guid>
    */
    add(shapeType: ShapeType, x: number, y: number): Promise<string>;
    /**
    * @param shapeId Transpied from System.Guid
    * @returns Transpied from System.Threading.Tasks.Task
    */
    move(shapeId: string): Promise<void>;
    /**
    * @param x Transpied from float
    * @param y Transpied from float
    * @returns Transpied from System.Threading.Tasks.Task
    */
    writeLine(x: number, y: number): Promise<void>;
}

export type IWhiteboardReceiver = {
    /**
    * @param shapeId Transpied from System.Guid
    * @param x Transpied from float
    * @param y Transpied from float
    * @returns Transpied from System.Threading.Tasks.Task
    */
    onMove(shapeId: string, x: number, y: number): Promise<void>;
    /**
    * @param x Transpied from float
    * @param y Transpied from float
    * @returns Transpied from System.Threading.Tasks.Task
    */
    onWriteLine(x: number, y: number): Promise<void>;
}

