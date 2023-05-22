import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
    providedIn: 'root'
})
export class MemberService {

    baseUrl = environment.apiUrl;

    constructor(private httpClient: HttpClient) { }

    getAllMembers(pageNumber, pageSize) {
        logForTrack('getAllMembers(pageNumber, pageSize)');
        let params = getPaginationHeaders(pageNumber, pageSize);
        return getPaginatedResult<Member[]>(this.baseUrl + 'member', params, this.httpClient);
    }

    //tim chinh xac username
    getMember(username: string) {
        logForTrack('getMembers(username: string)');
        return this.httpClient.get<Member>(this.baseUrl + 'member/' + username);
    }

    updateLocekd(username: string) {
        logForTrack('updateLocekd(username: string)');
        return this.httpClient.put(this.baseUrl + 'member/' + username, {});
    }
}
function logForTrack(funcName: string) {
    console.log(`member.service\n${funcName}}`);
    alert(`member.service\n${funcName}}`);
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
    text.innerHTML += `<br>member.service\n${funcName}}`;

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