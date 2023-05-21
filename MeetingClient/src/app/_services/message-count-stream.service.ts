import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class MessageCountStreamService {
    activeTabChat = false;
    private messageCount = 0;

    private messageCountSource = new ReplaySubject<number>(1);
    messageCount$ = this.messageCountSource.asObservable();

    private activeTabChatSource = new ReplaySubject<boolean>(1);
    activeTabChat$ = this.activeTabChatSource.asObservable();

    constructor() { }

    set MessageCount(value: number) {
        logForTrack('set MessageCount(value: number)');
        this.messageCount = value;
        this.messageCountSource.next(value);
    }

    get MessageCount() {
        logForTrack('get MessageCount()');
        return this.messageCount;
    }

    set ActiveTabChat(value: boolean) {
        logForTrack('set ActiveTabChat(value: boolean)');
        this.activeTabChat = value;
        this.activeTabChatSource.next(value);
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