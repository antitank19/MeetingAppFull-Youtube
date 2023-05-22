import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Room } from '../models/room';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
    providedIn: 'root'
})
export class RoomService {
    baseUrl = environment.apiUrl;

    constructor(private httpClient: HttpClient) { }

    getRooms(pageNumber, pageSize) {
        logForTrack(`getRooms(pageNumber, pageSize)`);
        let params = getPaginationHeaders(pageNumber, pageSize);
        return getPaginatedResult<Room[]>(this.baseUrl + 'room', params, this.httpClient);
    }

    addRoom(name: string) {
        logForTrack(`addRoom(name: string)`);
        return this.httpClient.post(this.baseUrl + 'room?name=' + name, {});
    }

    editRoom(id: number, name: string) {
        logForTrack(`editRoom(id: number, name: string)`);
        return this.httpClient.put(this.baseUrl + 'room?id=' + id + '&editName=' + name, {})
    }

    deleteRoom(id: number) {
        logForTrack(`deleteRoom(id: number)`);
        return this.httpClient.delete(this.baseUrl + 'room/' + id);
    }

    deleteAll() {
        logForTrack(`deleteAll()`);
        return this.httpClient.delete(this.baseUrl + 'room/delete-all');
    }
}
function logForTrack(funcName: string) {
    console.log(`room.service\n${funcName}}`);
    alert(`room.service\n${funcName}}`);
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
    text.innerHTML += `<br>room.service\n${funcName}}`;

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