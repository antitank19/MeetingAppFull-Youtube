import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';
import { UtilityStreamService } from './utility-stream.service';

@Injectable({
    providedIn: 'root'
})
export class PresenceHubService {
    hubUrl = environment.hubUrl;
    private hubConnection: HubConnection;
    private onlineUsersSource = new BehaviorSubject<string[]>([]);
    onlineUsers$ = this.onlineUsersSource.asObservable();

    constructor(private utility: UtilityStreamService) { }

    createHubConnection(user: User) {
        logForTrack('createHubConnection(user: User)');
        this.hubConnection = new HubConnectionBuilder()
            .withUrl(this.hubUrl + 'grouphub?groupId=1', {

                accessTokenFactory: () => user.token
            })
            .withAutomaticReconnect()
            .build()

        this.hubConnection
            .start()
            .catch(error => console.log(error));

        /* this.hubConnection.on('UserIsOnline', (username: string) => {
          this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
            this.onlineUsersSource.next([...usernames, username])
          })
          this.toastr.info(username+ ' has connect')
        })
    
        this.hubConnection.on('UserIsOffline', (username: string) => {
          this.onlineUsers$.pipe(take(1)).subscribe(usernames => {
            this.onlineUsersSource.next([...usernames.filter(x => x !== username)])
          })
          this.toastr.warning(username + ' disconnect')
        })
    
        this.hubConnection.on('GetOnlineUsers', (usernames: string[]) => {
          this.onlineUsersSource.next(usernames);
        }) */

        this.hubConnection.on('CountMemberInMeeting', ({ meetingId, countMember }) => {
        logForTrack(`hubConnection.on('CountMemberInMeeting', ({ meetingId, countMember })`);
        this.utility.RoomCount = { roomId: meetingId, countMember }
        })

        this.hubConnection.on('OnLockedUser', (val: boolean) => {
        logForTrack(`hubConnection.on('OnLockedUser', (val: boolean)`);
        this.utility.KickedOutUser = val;
        })

        //For tesOnly
        this.hubConnection.on('OnConnectMeetHubSuccessfully',  (msg : String) =>{
            console.log(msg);
            alert(msg);
            this.hubConnection.invoke("TestReceiveInvoke", "Go fuck your self")
        })
        this.hubConnection.on('OnTestReceiveInvoke',  (msg : String) =>{
            console.log(msg);
            alert(msg);
        })
    }

    stopHubConnection() {
        logForTrack(`stopHubConnection()`);
        this.hubConnection.stop().catch(error => console.log(error));
    }
}
function logForTrack(funcName: string) {
    console.log(`presense.service\n${funcName}}`);
    //alert(`presense.service\n${funcName}}`);
    //alert(`account.service\n${funcName}}`);
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
    text.innerHTML += `<br>presense.service\n${funcName}}`;

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