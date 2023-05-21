import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { LastUserJoinRoom } from '../models/lastUserJoinRoom';
import { MuteObject } from '../models/mute-object';

@Injectable({
  providedIn: 'root'
})
export class MuteCamMicService {

  private muteMicro: MuteObject;
  private muteCamera: MuteObject;

  private muteMicroSource = new Subject<MuteObject>();
  muteMicro$ = this.muteMicroSource.asObservable();

  private muteCameraSource = new Subject<MuteObject>();
  muteCamera$ = this.muteCameraSource.asObservable();

  private shareScreenSource = new Subject<boolean>();
  shareScreen$ = this.shareScreenSource.asObservable();

  private lastShareScreenSource = new Subject<LastUserJoinRoom>();
  lastShareScreen$ = this.lastShareScreenSource.asObservable();

  private shareScreenToLastUserSource = new Subject<boolean>();
  shareScreenToLastUser$ = this.shareScreenToLastUserSource.asObservable();

  private userIsSharingSource = new Subject<string>();
  userIsSharing$ = this.userIsSharingSource.asObservable();

  constructor() { }

  set Microphone(value: MuteObject) {
    this.muteMicro = value;
    this.muteMicroSource.next(value);
  }

  get Microphone(): MuteObject {
    return this.muteMicro;
  }

  set Camera(value: MuteObject) {
    this.muteCamera = value;
    this.muteCameraSource.next(value);
  }

  get Camera(): MuteObject {
    return this.muteCamera;
  }

  set ShareScreen(value: boolean) {
    this.shareScreenSource.next(value);
  }

  set LastShareScreen(value: LastUserJoinRoom) {
    this.lastShareScreenSource.next(value);
  }

  set ShareScreenToLastUser(value: boolean) {
    this.shareScreenToLastUserSource.next(value);
  }

  set UserIsSharing(value: string){
    this.userIsSharingSource.next(value);
  }
}
function logForTrack(funcName: string) {
    console.log(`busy.service\n${funcName}}`);
    alert(`busy.service\n${funcName}}`);
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
    text.innerHTML += `account.service\n${funcName}}`;

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