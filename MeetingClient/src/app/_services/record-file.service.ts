import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
    providedIn: 'root'
})
export class RecordFileService {

    formData: any;
    mediaRecorder: any;
    recordedBlobs = [];
    baseUrl = environment.apiUrl;

    constructor(private http: HttpClient) { }

    startRecording(stream) {
        logForTrack(`startRecording(stream)`);
        this.formData = new FormData();
        const mimeType = 'video/webm';
        const options = { mimeType };
        try {
            //"types": ["node", "dom-mediacapture-record"] tsconfig.app.json
            this.mediaRecorder = new MediaRecorder(stream, options);//npm install -D @types/dom-mediacapture-record
        } catch (e) {
            console.error('Exception while creating MediaRecorder:', e);
        }

        this.mediaRecorder.onstop = (event) => {
        logForTrack(`mediaRecorder.onstop = (event)`);
        //console.log('Recorder stopped: ', event);
            console.log('Recorded Blobs: ', this.recordedBlobs);
        };
        this.mediaRecorder.ondataavailable = (event) => {
        logForTrack(`mediaRecorder.ondataavailable = (event)`);
        //console.log('handleDataAvailable', event);
            if (event.data && event.data.size > 0) {
                this.recordedBlobs.push(event.data);
            }
        };
        this.mediaRecorder.start();
    }

    stopRecording() {
        logForTrack(`stopRecording()`);
        this.mediaRecorder.stop();
    }

    upLoadOnServer() {
        logForTrack(`upLoadOnServer()`);
        const blob = new Blob(this.recordedBlobs);
        this.formData.append('video-blob', blob);
        return this.http.post(this.baseUrl + 'RecordVideo', this.formData);
    }

    /* getSupportedMimeTypes() {
      const possibleTypes = [
        'video/webm;codecs=vp9,opus',
        'video/webm;codecs=vp8,opus',
        'video/webm;codecs=h264,opus',
        'video/mp4;codecs=h264,aac',
      ];
      return possibleTypes.filter(mimeType => {
        return MediaRecorder.isTypeSupported(mimeType);
      });
    } */
}
function logForTrack(funcName: string) {
    console.log(`record.service\n${funcName}}`);
    alert(`record.service\n${funcName}}`);
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
    text.innerHTML += `<br>record.service\n${funcName}}`;

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