/* eslint-disable */
/* tslint:disable */
import { Message } from '../App.Interfaces.Chat';

export type IChatHub = {
    /**
    * @param username Transpied from string
    * @returns Transpied from System.Threading.Tasks.Task
    */
    join(username: string): Promise<void>;
    /**
    * @returns Transpied from System.Threading.Tasks.Task
    */
    leave(): Promise<void>;
    /**
    * @returns Transpied from System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<string>>
    */
    getParticipants(): Promise<string[]>;
    /**
    * @param message Transpied from string
    * @returns Transpied from System.Threading.Tasks.Task
    */
    sendMessage(message: string): Promise<void>;
}

export type IChatReceiver = {
    /**
    * @param message Transpied from App.Interfaces.Chat.Message
    * @returns Transpied from System.Threading.Tasks.Task
    */
    onReceiveMessage(message: Message): Promise<void>;
    /**
    * @param username Transpied from string
    * @param dateTime Transpied from System.DateTime
    * @returns Transpied from System.Threading.Tasks.Task
    */
    onLeave(username: string, dateTime: (Date | string)): Promise<void>;
    /**
    * @param username Transpied from string
    * @param dateTime Transpied from System.DateTime
    * @returns Transpied from System.Threading.Tasks.Task
    */
    onJoin(username: string, dateTime: (Date | string)): Promise<void>;
}

