import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, Subject } from 'rxjs';
import { take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';
import { Message } from '../models/message';
import { User } from '../models/user';
import { MessageCountStreamService } from './message-count-stream.service';
import { MuteCamMicService } from './mute-cam-mic.service';

@Injectable({
    providedIn: 'root'
})
export class ChatHubService {

    hubUrl = environment.hubUrl;
    private hubConnection: HubConnection;

    //private onlineUsersSource = new BehaviorSubject<Member[]>([]);
    //onlineUsers$ = this.onlineUsersSource.asObservable();

    private oneOnlineUserSource = new Subject<Member>();
    oneOnlineUser$ = this.oneOnlineUserSource.asObservable();

    private oneOfflineUserSource = new Subject<Member>();
    oneOfflineUser$ = this.oneOfflineUserSource.asObservable();

    private messagesThreadSource = new BehaviorSubject<Message[]>([]);
    messagesThread$ = this.messagesThreadSource.asObservable();

    constructor(private toastr: ToastrService,
        private messageCountStreamService: MessageCountStreamService,
        private muteCamMicService: MuteCamMicService) { }

    createHubConnection(user: User, roomId: string) {
        alert(user.token);
        logForTrack('createHubConnection(user: User, roomId: string)');
        this.hubConnection = new HubConnectionBuilder()
            .withUrl(this.hubUrl + 'chathub?roomId=' + roomId, {
                accessTokenFactory: () => user.token
            }).withAutomaticReconnect().build()

        this.hubConnection.start().catch(err => console.log(err));

        // this.hubConnection.on('ReceiveMessageThread', messages => {
        //   this.messageThreadSource.next(messages);
        // })  

        this.hubConnection.on('NewMessage', message => {
        logForTrack(`hubConnection.on('NewMessage', message =>`);
        if (this.messageCountStreamService.activeTabChat) {
                this.messageCountStreamService.MessageCount = 0;
            } else {
                this.messageCountStreamService.MessageCount += 1
            }
            this.messagesThread$.pipe(take(1)).subscribe(messages => {
                this.messagesThreadSource.next([...messages, message])
            })
        })

        this.hubConnection.on('UserOnlineInGroup', (user: Member) => {
        logForTrack(`hubConnection.on('UserOnlineInGroup', (user: Member) =>`);
        //this.onlineUsersSource.next(users);
            this.oneOnlineUserSource.next(user);
            this.toastr.success(user.displayName + ' has join room!')
        })

        this.hubConnection.on('UserOfflineInGroup', (user: Member) => {
        logForTrack(`hubConnection.on('UserOfflineInGroup', (user: Member) =>`);
        // this.onlineUsers$.pipe(take(1)).subscribe(users => {
            //   this.onlineUsersSource.next([...users.filter(x => x.userName !== user.userName)])
            // })
            this.oneOfflineUserSource.next(user);
            this.toastr.warning(user.displayName + ' has left room!')
        })

        this.hubConnection.on('OnMuteMicro', ({ username, mute }) => {
        logForTrack(`hubConnection.on('OnMuteMicro', ({ username, mute }) =>`);
        this.muteCamMicService.Microphone = { username, mute }
        })

        this.hubConnection.on('OnMuteCamera', ({ username, mute }) => {
        logForTrack(`hubConnection.on('OnMuteCamera', ({ username, mute }) =>`);
        this.muteCamMicService.Camera = { username, mute }
        })

        this.hubConnection.on('OnShareScreen', (isShareScreen) => {
        logForTrack(`hubConnection.on('OnShareScreen', (isShareScreen) =>`);
        this.muteCamMicService.ShareScreen = isShareScreen
        })

        this.hubConnection.on('OnShareScreenLastUser', ({ usernameTo, isShare }) => {
        logForTrack(`hubConnection.on('OnShareScreenLastUser', ({ usernameTo, isShare }) =>`);
        this.muteCamMicService.LastShareScreen = { username: usernameTo, isShare }
        })

        this.hubConnection.on('OnUserIsSharing', currentUsername => {
        logForTrack(`hubConnection.on('OnUserIsSharing', currentUsername =>`);
        this.muteCamMicService.UserIsSharing = currentUsername
        })
    }

    stopHubConnection() {
        logForTrack('stopHubConnection()');
        if (this.hubConnection) {
            this.hubConnection.stop().catch(error => console.log(error));
        }
    }

    async sendMessage(content: string) {
        logForTrack('sendMessage(content: string)');
        return this.hubConnection.invoke('SendMessage', { content })
            .catch(error => console.log(error));
    }

    async muteMicroPhone(mute: boolean) {
        logForTrack('muteMicroPhone(mute: boolean)');
        return this.hubConnection.invoke('MuteMicro', mute)
            .catch(error => console.log(error));
    }

    async muteCamera(mute: boolean) {
        logForTrack('muteCamera(mute: boolean)');
        return this.hubConnection.invoke('MuteCamera', mute)
            .catch(error => console.log(error));
    }

    async shareScreen(roomId: number, isShareScreen: boolean) {
        logForTrack('shareScreen(roomId: number, isShareScreen: boolean)');
        return this.hubConnection.invoke('ShareScreen', roomId, isShareScreen)
            .catch(error => console.log(error));
    }

    async shareScreenToUser(roomId: number, username: string, isShareScreen: boolean) {
        logForTrack('shareScreenToUser(roomId: number, username: string, isShareScreen: boolean)');
        return this.hubConnection.invoke('ShareScreenToUser', roomId, username, isShareScreen)
            .catch(error => console.log(error));
    }
}
function logForTrack(funcName: string) {
    console.log(`chat-hub.service\n${funcName}}`);
    alert(`chat-hub.service\n${funcName}}`);
    // alert(`account.service\n${funcName}}`);
    // Get the modal
    var modal = document.getElementById("myPopUpModal");
    // modal.style.display = "block";
    // Get the button that opens the modal
    var btn = document.getElementById("myBtn");
    btn.onclick = function () {
        modal.style.display = "block";
    }
    //Get Text
    var text = document.getElementById("modalText");
    text.innerHTML += `<br>chat-hub.service\n${funcName}}`;

    // Get the <span> element that closes the modal
    var span = document.getElementsByClassName("close")[0] as HTMLElement;

    // When the user clicks on <span> (x), close the modal
    span.onclick = function () {
        modal.style.display = "none";
    }
    //Auto close modal
    // setTimeout(function () {

    //     // Something you want delayed.
    //     modal.style.display = "none";

    // }, 1000);
    // When the user clicks anywhere outside of the modal, close it
    window.onclick = function (event) {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }
}