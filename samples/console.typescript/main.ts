import { HubConnectionBuilder } from "@microsoft/signalr";
import { Message } from "./generated/App.Interfaces.Chat";
import { getHubProxyFactory, getReceiverRegister } from "./generated/TypedSignalR.Client";
import { IChatReceiver } from "./generated/TypedSignalR.Client/App.Interfaces.Chat";
import { createInterface } from 'readline';

console.log("==== start app ====")

const rl = createInterface({
    input: process.stdin,
});

const readline = rl[Symbol.asyncIterator]();

const connection = new HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/chathub")
    .build();

const receiver: IChatReceiver = {
    onReceiveMessage: (message: Message): Promise<void> => {
        console.log(`OnReceiveMessage`)
        console.log(`    username  : ${message.username}`)
        console.log(`    timeStamp : ${message.timeStamp}`)
        console.log(`    Message   : ${message.content}`)
        console.log()

        return Promise.resolve();
    },

    onLeave: (username: string, dateTime: string | Date): Promise<void> => {
        console.log(`OnLeave`)
        console.log(`    Username : ${username}`)
        console.log(`    Message  : ${dateTime}`)
        console.log()

        return Promise.resolve();
    },

    onJoin: (username: string, dateTime: string | Date): Promise<void> => {
        console.log(`OnJoin`)
        console.log(`    Username : ${username}`)
        console.log(`    Message  : ${dateTime}`)
        console.log()

        return Promise.resolve();
    }
}

const hubProxy = getHubProxyFactory("IChatHub")
    .createHubProxy(connection);

const subscription = getReceiverRegister("IChatReceiver")
    .register(connection, receiver)

console.log("Input username")

const username = (await readline.next()).value as string;

await connection.start()

await hubProxy.join(username)

const participants = await hubProxy.getParticipants()

console.log(`Participants`)

for (let it of participants) {
    console.log(`    ${it}`)
}

let flag = true;

process.on('SIGINT', async () => {
    console.log("dispose subscription...")
    subscription.dispose()

    console.log("stop connection...")
    await connection.stop()
    rl.close()
    flag = false
});

console.log("Input Message! > ")

for await (let message of readline) {
    if (!flag) {
        break
    }

    await hubProxy.sendMessage(message)
    console.log("Input Message! > ")
}

