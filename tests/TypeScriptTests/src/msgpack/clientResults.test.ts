import { HubConnectionBuilder } from '@microsoft/signalr'
import { MessagePackHubProtocol } from '@microsoft/signalr-protocol-msgpack';
import { getHubProxyFactory, getReceiverRegister } from '../generated/msgpack/TypedSignalR.Client'
import { Person } from '../generated/msgpack/TypedSignalR.Client.TypeScript.Tests.Shared';
import { IClientResultsTestHubReceiver, IReceiver } from '../generated/msgpack/TypedSignalR.Client/TypedSignalR.Client.TypeScript.Tests.Shared';

const testMethod = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/ClientResultsTestHub")
        .withHubProtocol(new MessagePackHubProtocol())
        .build();

    const receiver: IClientResultsTestHubReceiver = {
        GetGuidFromClient: (): Promise<string> => {
            return Promise.resolve("ba3088bb-e7ea-4924-b01b-695e879bb166");
        },
        GetPersonFromClient: (): Promise<Person> => {
            const person: Person = {
                Id: "c2368532-2f13-4079-9631-a38a048d84e1",
                Name: "Nana Daiba",
                Number: 7
            }
            return Promise.resolve(person);
        },
        SumInClient: (left: number, right: number): Promise<number> => {
            return Promise.resolve(left + right);
        }
    }

    const hubProxy = getHubProxyFactory("IClientResultsTestHub")
        .createHubProxy(connection);

    const subscription = getReceiverRegister("IClientResultsTestHubReceiver")
        .register(connection, receiver);

    await connection.start();
    const result = await hubProxy.StartTest();

    expect(result).toEqual(true);

    subscription.dispose();
    await connection.stop()
}

test('clientResults.test', testMethod);
