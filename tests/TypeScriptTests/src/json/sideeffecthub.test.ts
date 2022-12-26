import { HubConnectionBuilder } from '@microsoft/signalr'
import { getHubProxyFactory } from '../generated/json/TypedSignalR.Client'
import { UserDefinedType } from '../generated/json/TypedSignalR.Client.TypeScript.Tests.Shared';
import crypto from 'crypto'

const toUTCString = (date: string | Date): string => {
    if (typeof date === 'string') {
        const d = new Date(date);
        return d.toUTCString();
    }

    return date.toUTCString();
}

const testMethod = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/realtime/sideeffecthub")
        .build();

    const hubProxy = getHubProxyFactory("ISideEffectHub")
        .createHubProxy(connection);

    await connection.start();

    await hubProxy.init()
    await hubProxy.increment();
    await hubProxy.increment();
    await hubProxy.increment();
    await hubProxy.increment();

    const r1 = await hubProxy.result();

    expect(r1).toEqual(4);

    const list: UserDefinedType[] = []

    for (let i = 0; i < 10; i++) {
        var instance: UserDefinedType = {
            guid: crypto.randomUUID(),
            dateTime: new Date(),
        };

        list.push(instance);
        await hubProxy.post(instance);
    }

    const data = await hubProxy.fetch();

    for (let i = 0; i < list.length; i++) {
        const it1 = list[i];
        const it2 = data[i]
        
        it1.dateTime = toUTCString(it1.dateTime)
        it2.dateTime = toUTCString(it2.dateTime)

        expect(it1).toEqual(it2)
    }

    await connection.stop();
}

test('sideeffecthub.test', testMethod);
