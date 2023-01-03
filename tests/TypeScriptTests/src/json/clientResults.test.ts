import { HubConnectionBuilder } from '@microsoft/signalr'
import { getHubProxyFactory, getReceiverRegister } from '../generated/json/TypedSignalR.Client'
import { Person } from '../generated/json/TypedSignalR.Client.TypeScript.Tests.Shared';
import { IClientResultsTestHubReceiver, IReceiver } from '../generated/json/TypedSignalR.Client/TypedSignalR.Client.TypeScript.Tests.Shared';

const testMethod = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/ClientResultsTestHub")
        .build();

    const receiver: IClientResultsTestHubReceiver = {
        getGuidFromClient: (): Promise<string> => {
            return Promise.resolve("ba3088bb-e7ea-4924-b01b-695e879bb166");
        },
        getPersonFromClient: (): Promise<Person> => {
            const person: Person = {
                id: "c2368532-2f13-4079-9631-a38a048d84e1",
                name: "Nana Daiba",
                number: 7
            }
            return Promise.resolve(person);
        },
        sumInClient: (left: number, right: number): Promise<number> => {
            return Promise.resolve(left + right);
        }
    }

    const hubProxy = getHubProxyFactory("IClientResultsTestHub")
        .createHubProxy(connection);

    const subscription = getReceiverRegister("IClientResultsTestHubReceiver")
        .register(connection, receiver);

    await connection.start();
    const result = await hubProxy.startTest();

    expect(result).toEqual(true);

    subscription.dispose();
    await connection.stop()
}

test('clientResults.test', testMethod);
