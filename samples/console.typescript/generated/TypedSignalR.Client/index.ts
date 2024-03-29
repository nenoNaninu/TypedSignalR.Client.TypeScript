/* THIS (.ts) FILE IS GENERATED BY TypedSignalR.Client.TypeScript */
/* eslint-disable */
/* tslint:disable */
import { HubConnection, IStreamResult, Subject } from '@microsoft/signalr';
import { IChatHub, IChatReceiver } from './App.Interfaces.Chat';
import { IWhiteboardHub, IWhiteboardReceiver } from './App.Interfaces.WhiteBoard';
import { ShapeType } from '../App.Interfaces.WhiteBoard';
import { Message } from '../App.Interfaces.Chat';


// components

export type Disposable = {
    dispose(): void;
}

export type HubProxyFactory<T> = {
    createHubProxy(connection: HubConnection): T;
}

export type ReceiverRegister<T> = {
    register(connection: HubConnection, receiver: T): Disposable;
}

type ReceiverMethod = {
    methodName: string,
    method: (...args: any[]) => void
}

class ReceiverMethodSubscription implements Disposable {

    public constructor(
        private connection: HubConnection,
        private receiverMethod: ReceiverMethod[]) {
    }

    public readonly dispose = () => {
        for (const it of this.receiverMethod) {
            this.connection.off(it.methodName, it.method);
        }
    }
}

// API

export type HubProxyFactoryProvider = {
    (hubType: "IChatHub"): HubProxyFactory<IChatHub>;
    (hubType: "IWhiteboardHub"): HubProxyFactory<IWhiteboardHub>;
}

export const getHubProxyFactory = ((hubType: string) => {
    if(hubType === "IChatHub") {
        return IChatHub_HubProxyFactory.Instance;
    }
    if(hubType === "IWhiteboardHub") {
        return IWhiteboardHub_HubProxyFactory.Instance;
    }
}) as HubProxyFactoryProvider;

export type ReceiverRegisterProvider = {
    (receiverType: "IChatReceiver"): ReceiverRegister<IChatReceiver>;
    (receiverType: "IWhiteboardReceiver"): ReceiverRegister<IWhiteboardReceiver>;
}

export const getReceiverRegister = ((receiverType: string) => {
    if(receiverType === "IChatReceiver") {
        return IChatReceiver_Binder.Instance;
    }
    if(receiverType === "IWhiteboardReceiver") {
        return IWhiteboardReceiver_Binder.Instance;
    }
}) as ReceiverRegisterProvider;

// HubProxy

class IChatHub_HubProxyFactory implements HubProxyFactory<IChatHub> {
    public static Instance = new IChatHub_HubProxyFactory();

    private constructor() {
    }

    public readonly createHubProxy = (connection: HubConnection): IChatHub => {
        return new IChatHub_HubProxy(connection);
    }
}

class IChatHub_HubProxy implements IChatHub {

    public constructor(private connection: HubConnection) {
    }

    public readonly join = async (username: string): Promise<void> => {
        return await this.connection.invoke("Join", username);
    }

    public readonly leave = async (): Promise<void> => {
        return await this.connection.invoke("Leave");
    }

    public readonly getParticipants = async (): Promise<string[]> => {
        return await this.connection.invoke("GetParticipants");
    }

    public readonly sendMessage = async (message: string): Promise<void> => {
        return await this.connection.invoke("SendMessage", message);
    }
}

class IWhiteboardHub_HubProxyFactory implements HubProxyFactory<IWhiteboardHub> {
    public static Instance = new IWhiteboardHub_HubProxyFactory();

    private constructor() {
    }

    public readonly createHubProxy = (connection: HubConnection): IWhiteboardHub => {
        return new IWhiteboardHub_HubProxy(connection);
    }
}

class IWhiteboardHub_HubProxy implements IWhiteboardHub {

    public constructor(private connection: HubConnection) {
    }

    public readonly add = async (shapeType: ShapeType, x: number, y: number): Promise<string> => {
        return await this.connection.invoke("Add", shapeType, x, y);
    }

    public readonly move = async (shapeId: string): Promise<void> => {
        return await this.connection.invoke("Move", shapeId);
    }

    public readonly writeLine = async (x: number, y: number): Promise<void> => {
        return await this.connection.invoke("WriteLine", x, y);
    }
}


// Receiver

class IChatReceiver_Binder implements ReceiverRegister<IChatReceiver> {

    public static Instance = new IChatReceiver_Binder();

    private constructor() {
    }

    public readonly register = (connection: HubConnection, receiver: IChatReceiver): Disposable => {

        const __onReceiveMessage = (...args: [Message]) => receiver.onReceiveMessage(...args);
        const __onLeave = (...args: [string, (Date | string)]) => receiver.onLeave(...args);
        const __onJoin = (...args: [string, (Date | string)]) => receiver.onJoin(...args);

        connection.on("OnReceiveMessage", __onReceiveMessage);
        connection.on("OnLeave", __onLeave);
        connection.on("OnJoin", __onJoin);

        const methodList: ReceiverMethod[] = [
            { methodName: "OnReceiveMessage", method: __onReceiveMessage },
            { methodName: "OnLeave", method: __onLeave },
            { methodName: "OnJoin", method: __onJoin }
        ]

        return new ReceiverMethodSubscription(connection, methodList);
    }
}

class IWhiteboardReceiver_Binder implements ReceiverRegister<IWhiteboardReceiver> {

    public static Instance = new IWhiteboardReceiver_Binder();

    private constructor() {
    }

    public readonly register = (connection: HubConnection, receiver: IWhiteboardReceiver): Disposable => {

        const __onMove = (...args: [string, number, number]) => receiver.onMove(...args);
        const __onWriteLine = (...args: [number, number]) => receiver.onWriteLine(...args);

        connection.on("OnMove", __onMove);
        connection.on("OnWriteLine", __onWriteLine);

        const methodList: ReceiverMethod[] = [
            { methodName: "OnMove", method: __onMove },
            { methodName: "OnWriteLine", method: __onWriteLine }
        ]

        return new ReceiverMethodSubscription(connection, methodList);
    }
}

