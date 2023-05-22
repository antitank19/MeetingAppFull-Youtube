import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  busyRequestCount = 0;

  constructor(private spinnerService: NgxSpinnerService) { }

  busy() {
    // logForTrack('busy()');
    this.busyRequestCount++;
    this.spinnerService.show(undefined, {
      type: 'line-scale-party',
      bdColor: 'rgba(255,255,255,0)',
      color: '#333333'
    });
  }

  idle() {
    // logForTrack('idle()');
    this.busyRequestCount--;
    if (this.busyRequestCount <= 0) {
      this.busyRequestCount = 0;
      this.spinnerService.hide();
    }
  }
}
// function logForTrack(funcName: string) {
//     console.log(`busy.service\n${funcName}}`);
//     alert(`busy.service\n${funcName}}`);
//     // alert(`account.service\n${funcName}}`);
//     // Get the modal
//     var modal = document.getElementById("myPopUpModal");
//     // modal.style.display = "block";
//     // Get the button that opens the modal
//     var btn = document.getElementById("myBtn");
//     btn.onclick = function () {
//         modal.style.display = "block";
//     }
//     //Get Text
//     var text = document.getElementById("modalText");
//     text.innerHTML += `account.service\n${funcName}}`;

//     // Get the <span> element that closes the modal
//     var span = document.getElementsByClassName("close")[0] as HTMLElement;

//     // When the user clicks on <span> (x), close the modal
//     span.onclick = function () {
//         modal.style.display = "none";
//     }
//     //Auto close modal
//     // setTimeout(function () {

//     //     // Something you want delayed.
//     //     modal.style.display = "none";

//     // }, 1000);
//     // When the user clicks anywhere outside of the modal, close it
//     window.onclick = function (event) {
//         if (event.target == modal) {
//             modal.style.display = "none";
//         }
//     }
// }