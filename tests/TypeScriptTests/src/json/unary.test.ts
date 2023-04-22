import { HubConnectionBuilder } from '@microsoft/signalr'
import { getHubProxyFactory } from '../generated/json/TypedSignalR.Client'
import { MyEnum, UserDefinedType } from '../generated/json/TypedSignalR.Client.TypeScript.Tests.Shared';
import crypto from 'crypto'

const getRandomInt = (max: number) => {
    return Math.floor(Math.random() * max);
}

const toUTCString = (date: string | Date): string => {
    if (typeof date === 'string') {
        const d = new Date(date);
        return d.toUTCString();
    }

    return date.toUTCString();
}

const testMethod = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/unaryhub")
        .build();

    const hubProxy = getHubProxyFactory("IUnaryHub")
        .createHubProxy(connection);

    try {
        await connection.start();

        const r1 = await hubProxy.get();
        expect(r1).toEqual("TypedSignalR.Client.TypeScript");

        const x = getRandomInt(1000);
        const y = getRandomInt(1000);

        const r2 = await hubProxy.add(x, y);
        expect(r2).toEqual(x + y);

        const s1 = "revue";
        const s2 = "starlight";

        const r3 = await hubProxy.cat(s1, s2);;

        expect(r3).toEqual(s1 + s2);

        const instance: UserDefinedType = {
            dateTime: new Date(),
            guid: crypto.randomUUID()
        }

        const r4 = await hubProxy.echo(instance);

        instance.dateTime = toUTCString(r4.dateTime)
        r4.dateTime = toUTCString(r4.dateTime)

        expect(r4).toEqual(instance)

        const r5 = await hubProxy.echoMyEnum(MyEnum.Four);
        expect(r5).toEqual(MyEnum.Four)
    } catch {
        // eat exception
    }

    await connection.stop();
}

test('unary.test', testMethod);
