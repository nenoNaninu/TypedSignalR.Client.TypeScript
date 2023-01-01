import { HubConnectionBuilder } from "@microsoft/signalr";
import { MessagePackHubProtocol } from "@microsoft/signalr-protocol-msgpack";
import { getHubProxyFactory, getReceiverRegister } from "../generated/msgpack/TypedSignalR.Client";
import { Message, Person } from "../generated/msgpack/TypedSignalR.Client.TypeScript.Tests.Shared";
import { IStreamingHub } from "../generated/msgpack/TypedSignalR.Client/TypedSignalR.Client.TypeScript.Tests.Shared";

const persons: Person[] = [
    { Id: "c61bcc3f-f477-2206-3c1f-830b05a6ed0f", Name: "KAREN AIJO", Number: 1 },
    { Id: "ef28aba1-1f85-daf2-2dac-a5cf138421cd", Name: "FUTABA ISURUGI", Number: 2 },
    { Id: "478d3485-5914-1269-e4dc-dedcc749ebb6", Name: "CLAUDINU SAIJO", Number: 11 },
    { Id: "8fd696c1-b102-7aa6-259b-4f8772457a7a", Name: "NANA DAIBA", Number: 15 },
    { Id: "8fd696c1-b102-7aa6-259b-4f8772457a7a", Name: "MAHIRU TSUYUZAKI", Number: 17 },
    { Id: "ced51d7b-fe37-b619-4026-a39457c954b6", Name: "MAYA TENDO", Number: 18 },
    { Id: "60287e1f-7b34-59f9-47d1-c29ae758872e", Name: "KAORUKO HANAYAGI", Number: 22 },
    { Id: "5921c655-6012-48f1-50ef-1881277c22d4", Name: "JUNNA HOSIMI", Number: 25 },
    { Id: "36d8986d-17de-2442-a33e-18ec942575f1", Name: "HIKARI KAGURA", Number: 29 },
]

class PromiseCompletionSource {

    public readonly promise: Promise<void>;
    public reject = (reason?: any) => this._reject(reason);
    public resolve = (reason?: any) => this._resolve(reason);

    private _reject!: (reason?: any) => void;
    private _resolve!: (value: void | PromiseLike<void>) => void;

    constructor() {
        this.promise = new Promise<void>((resolve, reject) => {
            this._resolve = resolve;
            this._reject = reject;
        })
    }
}


const test_ZeroParameter = async (hubProxy: IStreamingHub) => {
    var pcs = new PromiseCompletionSource();
    const list: Person[] = [];

    hubProxy.ZeroParameter()
        .subscribe({
            next: (value: Person): void => {
                list.push(value);
            },
            error: (err: any): void => {
                list.length = 0;
                pcs.reject();
            },
            complete: (): void => {
                pcs.resolve();
            }
        });

    await pcs.promise;

    for (let i = 0; i < persons.length; i++) {
        expect(list[i]).toEqual(persons[i]);
    }
}


const test_CancellationTokenOnly = async (hubProxy: IStreamingHub) => {
    var pcs = new PromiseCompletionSource();
    const list: Person[] = [];

    hubProxy.CancellationTokenOnly()
        .subscribe({
            next: (value: Person): void => {
                list.push(value);
            },
            error: (err: any): void => {
                list.length = 0;
                pcs.reject();
            },
            complete: (): void => {
                pcs.resolve();
            }
        });

    await pcs.promise;

    for (let i = 0; i < persons.length; i++) {
        expect(list[i]).toEqual(persons[i]);
    }
}

const test_Counter = async (hubProxy: IStreamingHub) => {
    var pcs = new PromiseCompletionSource();

    const publisher: Person = {
        Id: "8fd696c1-b102-7aa6-259b-4f8772457a7a",
        Name: "NANA DAIBA",
        Number: 15
    }

    const list: Message[] = [];

    const step = 2;

    hubProxy.Counter(publisher, 0, step, 10)
        .subscribe({
            next: (value: Message): void => {
                list.push(value);
            },
            error: (err: any): void => {
                list.length = 0;
                pcs.reject();
            },
            complete: (): void => {
                pcs.resolve();
            }
        });

    await pcs.promise;

    let value = 0;

    for (let i = 0; i < persons.length; i++) {
        expect(list[i].Publisher).toEqual(publisher);
        expect(list[i].Value).toEqual(value);
        
        value += step;
    }
}

const test_CancelableCounter = async (hubProxy: IStreamingHub) => {
    var pcs = new PromiseCompletionSource();

    const publisher: Person = {
        Id: "8fd696c1-b102-7aa6-259b-4f8772457a7a",
        Name: "NANA DAIBA",
        Number: 15
    }

    const list: Message[] = [];

    const step = 2;

    hubProxy.CancelableCounter(publisher, 0, step, 10)
        .subscribe({
            next: (value: Message): void => {
                list.push(value);
            },
            error: (err: any): void => {
                list.length = 0;
                pcs.reject();
            },
            complete: (): void => {
                pcs.resolve();
            }
        });

    await pcs.promise;

    let value = 0;

    for (let i = 0; i < persons.length; i++) {
        expect(list[i].Publisher).toEqual(publisher);
        expect(list[i].Value).toEqual(value);
        
        value += step;
    }
}

const test_TaskCancelableCounter = async (hubProxy: IStreamingHub) => {
    var pcs = new PromiseCompletionSource();

    const publisher: Person = {
        Id: "8fd696c1-b102-7aa6-259b-4f8772457a7a",
        Name: "NANA DAIBA",
        Number: 15
    }

    const list: Message[] = [];

    const step = 2;

    hubProxy.TaskCancelableCounter(publisher, 0, step, 10)
        .subscribe({
            next: (value: Message): void => {
                list.push(value);
            },
            error: (err: any): void => {
                list.length = 0;
                pcs.reject();
            },
            complete: (): void => {
                pcs.resolve();
            }
        });

    await pcs.promise;

    let value = 0;

    for (let i = 0; i < persons.length; i++) {
        expect(list[i].Publisher).toEqual(publisher);
        expect(list[i].Value).toEqual(value);
        
        value += step;
    }
}

const test_ZeroParameterChannel = async (hubProxy: IStreamingHub) => {
    var pcs = new PromiseCompletionSource();
    const list: Person[] = [];

    hubProxy.ZeroParameterChannel()
        .subscribe({
            next: (value: Person): void => {
                list.push(value);
            },
            error: (err: any): void => {
                list.length = 0;
                pcs.reject();
            },
            complete: (): void => {
                pcs.resolve();
            }
        });

    await pcs.promise;

    for (let i = 0; i < persons.length; i++) {
        expect(list[i]).toEqual(persons[i]);
    }
}

const test_CancellationTokenOnlyChannel = async (hubProxy: IStreamingHub) => {
    var pcs = new PromiseCompletionSource();
    const list: Person[] = [];

    hubProxy.CancellationTokenOnlyChannel()
        .subscribe({
            next: (value: Person): void => {
                list.push(value);
            },
            error: (err: any): void => {
                list.length = 0;
                pcs.reject();
            },
            complete: (): void => {
                pcs.resolve();
            }
        });

    await pcs.promise;

    for (let i = 0; i < persons.length; i++) {
        expect(list[i]).toEqual(persons[i]);
    }
}

const test_CounterChannel = async (hubProxy: IStreamingHub) => {
    var pcs = new PromiseCompletionSource();

    const publisher: Person = {
        Id: "8fd696c1-b102-7aa6-259b-4f8772457a7a",
        Name: "NANA DAIBA",
        Number: 15
    }

    const list: Message[] = [];

    const step = 2;

    hubProxy.CounterChannel(publisher, 0, step, 10)
        .subscribe({
            next: (value: Message): void => {
                list.push(value);
            },
            error: (err: any): void => {
                list.length = 0;
                pcs.reject();
            },
            complete: (): void => {
                pcs.resolve();
            }
        });

    await pcs.promise;

    let value = 0;

    for (let i = 0; i < persons.length; i++) {
        expect(list[i].Publisher).toEqual(publisher);
        expect(list[i].Value).toEqual(value);
        
        value += step;
    }
}

const test_CancelableCounterChannel = async (hubProxy: IStreamingHub) => {
    var pcs = new PromiseCompletionSource();

    const publisher: Person = {
        Id: "8fd696c1-b102-7aa6-259b-4f8772457a7a",
        Name: "NANA DAIBA",
        Number: 15
    }

    const list: Message[] = [];

    const step = 2;

    hubProxy.CancelableCounterChannel(publisher, 0, step, 10)
        .subscribe({
            next: (value: Message): void => {
                list.push(value);
            },
            error: (err: any): void => {
                list.length = 0;
                pcs.reject();
            },
            complete: (): void => {
                pcs.resolve();
            }
        });

    await pcs.promise;

    let value = 0;

    for (let i = 0; i < persons.length; i++) {
        expect(list[i].Publisher).toEqual(publisher);
        expect(list[i].Value).toEqual(value);
        
        value += step;
    }
}

const testMethod1 = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/StreamingHub")
        .withHubProtocol(new MessagePackHubProtocol())
        .build();

    const hubProxy = getHubProxyFactory("IStreamingHub")
        .createHubProxy(connection);

    await connection.start();

    await test_ZeroParameter(hubProxy);
    await test_CancellationTokenOnly(hubProxy);

    await connection.stop()
}

const testMethod2 = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/StreamingHub")
        .withHubProtocol(new MessagePackHubProtocol())
        .build();

    const hubProxy = getHubProxyFactory("IStreamingHub")
        .createHubProxy(connection);

    await connection.start();

    await test_Counter(hubProxy);
    await test_CancelableCounter(hubProxy);
    await test_TaskCancelableCounter(hubProxy);

    await connection.stop()
}

const testMethod3 = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/StreamingHub")
        .withHubProtocol(new MessagePackHubProtocol())
        .build();

    const hubProxy = getHubProxyFactory("IStreamingHub")
        .createHubProxy(connection);
    await connection.start();

    await test_ZeroParameterChannel(hubProxy);
    await test_CancellationTokenOnlyChannel(hubProxy);

    await connection.stop()
}

const testMethod4 = async () => {
    const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5000/hubs/StreamingHub")
        .withHubProtocol(new MessagePackHubProtocol())
        .build();

    const hubProxy = getHubProxyFactory("IStreamingHub")
        .createHubProxy(connection);

    await connection.start();

    await test_CounterChannel(hubProxy);
    await test_CancelableCounterChannel(hubProxy);

    await connection.stop()
}

test('streaming1.test', testMethod1);
test('streaming2.test', testMethod2);
test('streaming3.test', testMethod3);
test('streaming4.test', testMethod4);
