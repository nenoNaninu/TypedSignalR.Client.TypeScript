import { HubConnectionBuilder } from '@microsoft/signalr'
import { getHubProxyFactory } from '../generated/msgpack/TypedSignalR.Client'
import { MyEnum, MyRequestItem, MyRequestItem2, UserDefinedType } from '../generated/msgpack/TypedSignalR.Client.TypeScript.Tests.Shared';
import crypto from 'crypto'
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';

const getRandomInt = (max: number) => {
    return Math.floor(Math.random() * max);
}

const testMethod = async () => {

    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/unaryhub")
        .withHubProtocol(new MessagePackHubProtocol())
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
            DateTime: new Date(),
            Guid: crypto.randomUUID()
        }

        const r4 = await hubProxy.echo(instance);

        instance.DateTime = r4.DateTime
        r4.DateTime = r4.DateTime

        expect(r4).toEqual(instance)

        const r5 = await hubProxy.echoMyEnum(MyEnum.Four);

        expect(r5).toEqual(MyEnum.Four)

        const array: MyRequestItem[] = []
        array.push({ Text: "melonpan" })
        array.push({ Text: "banana" })

        const r6 = await hubProxy.requestArray(array);

        expect(r6.length).toEqual(2)
        expect(r6[0].Text).toEqual("melonpanmelonpan")
        expect(r6[1].Text).toEqual("bananabanana")

        const list: MyRequestItem2[] = []
        list.push({ Id: "14ba25de-0a67-4713-8d29-59bcbec1c194" })
        list.push({ Id: "7e0ddf0a-2e55-4a32-98a0-049e12a4d728" })
        list.push({ Id: "b237bcb2-053a-4d4a-8868-6e78ca651ecd" })

        const r7 = await hubProxy.requestList(list);

        expect(r7.length).toEqual(2)
        expect(r7[0].Id).toEqual("b237bcb2-053a-4d4a-8868-6e78ca651ecd")
        expect(r7[1].Id).toEqual("7e0ddf0a-2e55-4a32-98a0-049e12a4d728")
        expect(r7[2].Id).toEqual("14ba25de-0a67-4713-8d29-59bcbec1c194")
    }
    finally {
        await connection.stop();
    }
}

test('unary.test', testMethod);
