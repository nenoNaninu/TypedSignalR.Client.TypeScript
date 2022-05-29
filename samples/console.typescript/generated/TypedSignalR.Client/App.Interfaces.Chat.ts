import { Message } from '../App.Interfaces.Chat';

export type IChatHub = {
    join(username: string): Promise<void>;
    leave(): Promise<void>;
    getParticipants(): Promise<string[]>;
    sendMessage(message: string): Promise<void>;
}

export type IChatReceiver = {
    onReceiveMessage(message: Message): Promise<void>;
    onLeave(username: string, dateTime: (Date | string)): Promise<void>;
    onJoin(username: string, dateTime: (Date | string)): Promise<void>;
}

