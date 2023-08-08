import { HubConnectionBuilder } from '@microsoft/signalr'
import { getHubProxyFactory } from '../generated/json/TypedSignalR.Client'
import { NestedTypeParentRequest } from '../generated/json/TypedSignalR.Client.TypeScript.Tests.Shared';

const testMethod = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/NestedTypeHub")
        .build();

    const hubProxy = getHubProxyFactory("INestedTypeHub")
        .createHubProxy(connection);

    try {
        await connection.start();
        const response = await hubProxy.get();

        expect(response.items.length).toEqual(3);
        expect(response.items[0].message).toEqual("KAREN AIJO");
        expect(response.items[0].value).toEqual(1);

        expect(response.items[1].message).toEqual("MAHIRU TSUYUZAKI");
        expect(response.items[1].value).toEqual(17);

        expect(response.items[2].message).toEqual("HIKARI KAGURA");
        expect(response.items[2].value).toEqual(29);

        const obj: NestedTypeParentRequest = {
            items: [
                {
                    value: 15,
                    message: "NANA DAIBA"
                },
                {
                    value: 18,
                    message: "MAYA TENDO"
                }
            ]
        }

        const result = await hubProxy.set(obj);

        expect(result).toEqual(99);
    }
    finally {
        await connection.stop()
    }
}

test('nestedTypeHub.test', testMethod);
