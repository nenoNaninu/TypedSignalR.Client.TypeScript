import { HubConnectionBuilder } from '@microsoft/signalr'
import { getHubProxyFactory } from '../generated/json/TypedSignalR.Client'
import { MyEnum, MyRequestItem, MyRequestItem2, UserDefinedType } from '../generated/json/TypedSignalR.Client.TypeScript.Tests.Shared';
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

        const array: MyRequestItem[] = []
        array.push({ text: "melonpan" })
        array.push({ text: "banana" })

        const r6 = await hubProxy.requestArray(array);

        expect(r6.length).toEqual(2)
        expect(r6[0].text).toEqual("melonpanmelonpan")
        expect(r6[1].text).toEqual("bananabanana")

        const list: MyRequestItem2[] = []
        list.push({ id: "14ba25de-0a67-4713-8d29-59bcbec1c194" })
        list.push({ id: "7e0ddf0a-2e55-4a32-98a0-049e12a4d728" })
        list.push({ id: "b237bcb2-053a-4d4a-8868-6e78ca651ecd" })

        const r7 = await hubProxy.requestList(list);

        expect(r7.length).toEqual(3)
        expect(r7[0].id).toEqual("b237bcb2-053a-4d4a-8868-6e78ca651ecd")
        expect(r7[1].id).toEqual("7e0ddf0a-2e55-4a32-98a0-049e12a4d728")
        expect(r7[2].id).toEqual("14ba25de-0a67-4713-8d29-59bcbec1c194")
    }
    finally {
        await connection.stop();
    }
}

test('unary.test', testMethod);
