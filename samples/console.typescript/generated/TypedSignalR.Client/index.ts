/* eslint-disable */
/* tslint:disable */
import { HubConnection } from '@microsoft/signalr';
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

    constructor(
        private connection: HubConnection,
        private receiverMethod: ReceiverMethod[]) {
    }

    dispose(): void {
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

    createHubProxy(connection: HubConnection): IChatHub {
        return new IChatHub_HubProxy(connection);
    }
}

class IChatHub_HubProxy implements IChatHub {

    constructor(private connection: HubConnection) {
    }

    join = async (username: string): Promise<void> => {
        return await this.connection.invoke("Join", username);
    }
    leave = async (): Promise<void> => {
        return await this.connection.invoke("Leave");
    }
    getParticipants = async (): Promise<string[]> => {
        return await this.connection.invoke("GetParticipants");
    }
    sendMessage = async (message: string): Promise<void> => {
        return await this.connection.invoke("SendMessage", message);
    }
}

class IWhiteboardHub_HubProxyFactory implements HubProxyFactory<IWhiteboardHub> {
    public static Instance = new IWhiteboardHub_HubProxyFactory();

    createHubProxy(connection: HubConnection): IWhiteboardHub {
        return new IWhiteboardHub_HubProxy(connection);
    }
}

class IWhiteboardHub_HubProxy implements IWhiteboardHub {

    constructor(private connection: HubConnection) {
    }

    add = async (shapeType: ShapeType, x: number, y: number): Promise<string> => {
        return await this.connection.invoke("Add", shapeType, x, y);
    }
    move = async (shapeId: string): Promise<void> => {
        return await this.connection.invoke("Move", shapeId);
    }
    writeLine = async (x: number, y: number): Promise<void> => {
        return await this.connection.invoke("WriteLine", x, y);
    }
}


// Receiver

class IChatReceiver_Binder implements ReceiverRegister<IChatReceiver>{

    public static Instance = new IChatReceiver_Binder();

    register(connection: HubConnection, receiver: IChatReceiver): Disposable {

        connection.on("OnReceiveMessage", receiver.onReceiveMessage);
        connection.on("OnLeave", receiver.onLeave);
        connection.on("OnJoin", receiver.onJoin);

        const methodList: ReceiverMethod[] = [
            { methodName: "OnReceiveMessage", method: receiver.onReceiveMessage },
            { methodName: "OnLeave", method: receiver.onLeave },
            { methodName: "OnJoin", method: receiver.onJoin }
        ]

        return new ReceiverMethodSubscription(connection, methodList);
    }
}

class IWhiteboardReceiver_Binder implements ReceiverRegister<IWhiteboardReceiver>{

    public static Instance = new IWhiteboardReceiver_Binder();

    register(connection: HubConnection, receiver: IWhiteboardReceiver): Disposable {

        connection.on("OnMove", receiver.onMove);
        connection.on("OnWriteLine", receiver.onWriteLine);

        const methodList: ReceiverMethod[] = [
            { methodName: "OnMove", method: receiver.onMove },
            { methodName: "OnWriteLine", method: receiver.onWriteLine }
        ]

        return new ReceiverMethodSubscription(connection, methodList);
    }
}

