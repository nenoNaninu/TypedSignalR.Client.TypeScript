import { HubConnectionBuilder } from '@microsoft/signalr'
import { getHubProxyFactory, getReceiverRegister } from './generated/TypedSignalR.Client'
import { UserDefinedType } from './generated/TypedSignalR.Client.TypeScript.Tests.Shared';
import { IReceiver } from './generated/TypedSignalR.Client/TypedSignalR.Client.TypeScript.Tests.Shared';

const toUTCString = (date: string | Date): string => {
    if (typeof date === 'string') {
        const d = new Date(date);
        return d.toUTCString();
    }

    return date.toUTCString();
}

const answerMessages: string[] = [
    "b1f7cd73-13b8-49bd-9557-ffb38859d18b",
    "3f5c3585-d01b-4f8f-8139-62a1241850e2",
    "92021a22-5823-4501-8cbd-c20d4ca6e54c",
    "5b134f73-2dc1-4271-8316-1a4250f42241",
    "e73acd30-e034-4569-8f30-88ac34b99052",
    "0d7531b5-0a36-4fe7-bbe5-8fee38c38c07",
    "32915627-3df6-41dc-8d30-7c655c2f7e61",
    "c875a6f9-9ddb-440b-a7e4-6e893f59ab9e",
];

const guids: string[] = [
    "b2f626e5-b4d4-4713-891d-f6cb107e502e",
    "22733524-2087-4701-a586-c6bf0ce36f74",
    "b89324bf-daf2-422a-85f2-6843b9c09b6a",
    "779769d1-0aee-4dba-82c7-9e1044836d75"
];

const dateTimes: string[] = [
    "2017-04-17",
    "2018-05-25",
    "2019-03-31",
    "2022-02-06",
];

const testMethod = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/realtime/receivertesthub")
        .build();

    const receiveMessageList: [string, number][] = [];
    const userDefinedList: UserDefinedType[] = [];
    let notifyCallCount = 0;

    const receiver: IReceiver = {
        receiveMessage: (message: string, value: number): Promise<void> => {
            receiveMessageList.push([message, value]);
            return Promise.resolve();
        },
        notify: (): Promise<void> => {
            notifyCallCount += 1;
            return Promise.resolve();
        },
        receiveCustomMessage: (userDefined: UserDefinedType): Promise<void> => {
            userDefinedList.push(userDefined)
            return Promise.resolve();
        }
    }

    const hubProxy = getHubProxyFactory("IReceiverTestHub")
        .createHubProxy(connection);

    const subscription = getReceiverRegister("IReceiver")
        .register(connection, receiver);

    await connection.start();
    await hubProxy.start();

    expect(notifyCallCount).toEqual(17);

    for (let i = 0; i < receiveMessageList.length; i++) {
        expect(receiveMessageList[i][0]).toEqual(answerMessages[i]);
        expect(receiveMessageList[i][1]).toEqual(i);
    }

    for (let i = 0; i < userDefinedList.length; i++) {
        expect(userDefinedList[i].guid).toEqual(guids[i]);
        expect(toUTCString(userDefinedList[i].dateTime)).toEqual(toUTCString(dateTimes[i]));
    }

    subscription.dispose();
    await connection.stop()
}

test('receiver.test', testMethod);
