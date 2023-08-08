import { HubConnectionBuilder } from '@microsoft/signalr'
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';
import { getHubProxyFactory } from '../generated/msgpack/TypedSignalR.Client'
import { NestedTypeParentRequest } from '../generated/msgpack/TypedSignalR.Client.TypeScript.Tests.Shared';

const testMethod = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/NestedTypeHub")
        .withHubProtocol(new MessagePackHubProtocol())
        .build();

    const hubProxy = getHubProxyFactory("INestedTypeHub")
        .createHubProxy(connection);

    try {
        await connection.start();
        const response = await hubProxy.get();

        expect(response.Items.length).toEqual(3);
        expect(response.Items[0].Message).toEqual("KAREN AIJO");
        expect(response.Items[0].Value).toEqual(1);

        expect(response.Items[1].Message).toEqual("MAHIRU TSUYUZAKI");
        expect(response.Items[1].Value).toEqual(17);

        expect(response.Items[2].Message).toEqual("HIKARI KAGURA");
        expect(response.Items[2].Value).toEqual(29);

        const obj: NestedTypeParentRequest = {
            Items: [
                {
                    Value: 15,
                    Message: "NANA DAIBA"
                },
                {
                    Value: 18,
                    Message: "MAYA TENDO"
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
