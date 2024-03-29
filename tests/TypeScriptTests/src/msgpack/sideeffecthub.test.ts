import { HubConnectionBuilder } from '@microsoft/signalr'
import { getHubProxyFactory } from '../generated/msgpack/TypedSignalR.Client'
import { UserDefinedType } from '../generated/msgpack/TypedSignalR.Client.TypeScript.Tests.Shared';
import crypto from 'crypto'
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';

const toUTCString = (date: string | Date): string => {
    if (typeof date === 'string') {
        const d = new Date(date);
        return d.toUTCString();
    }

    return date.toUTCString();
}

const testMethod = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/sideeffecthub")
        .withHubProtocol(new MessagePackHubProtocol())
        .build();

    const hubProxy = getHubProxyFactory("ISideEffectHub")
        .createHubProxy(connection);

    try {
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
                Guid: crypto.randomUUID(),
                DateTime: new Date(),
            };

            list.push(instance);
            await hubProxy.post(instance);
        }

        const data = await hubProxy.fetch();

        for (let i = 0; i < list.length; i++) {
            const it1 = list[i];
            const it2 = data[i]

            it1.DateTime = it1.DateTime
            it2.DateTime = it2.DateTime

            expect(it1).toEqual(it2)
        }
    }
    finally {
        await connection.stop();
    }
}

test('sideeffecthub.test', testMethod);
