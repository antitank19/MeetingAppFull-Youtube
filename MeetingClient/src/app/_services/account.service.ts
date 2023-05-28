import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';
import { map } from 'rxjs/operators';
import { PresenceService } from './presence.service';

@Injectable({
    providedIn: 'root'
})

export class AccountService {
    baseUrl = environment.apiUrl;
    private currentUserSource = new ReplaySubject<User>(1);
    currentUser$ = this.currentUserSource.asObservable();

    constructor(private http: HttpClient, private presence: PresenceService) { }

    login(model: any) {
        logForTrack('login(model: any)');
        return this.http.post(this.baseUrl + 'Account/login', model).pipe(
            map((res: User) => {
                const user = res;
                if (user) {
                    this.setCurrentUser(user);
                    this.presence.createHubConnection(user);
                }
            })
        )
    }

    loginWithSocial(model: any) {
        logForTrack('loginWithSocial(model: any)');
        return this.http.post(this.baseUrl + 'Account/login-social', model).pipe(
            map((res: User) => {
                const user = res;
                if (user) {
                    this.setCurrentUser(user);
                    this.presence.createHubConnection(user);
                }
            })
        )
    }

    setCurrentUser(user: User) {
        logForTrack('setCurrentUser(user: User)');
        if (user) {
            user.roles = [];
            const roles = this.getDecodedToken(user.token).role;//copy token to jwt.io see .role   
            Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);
            localStorage.setItem('user', JSON.stringify(user));
            this.currentUserSource.next(user);
        }
    }

    logout() {
        logForTrack('logout()');
        localStorage.removeItem('user');
        this.currentUserSource.next(null);
        this.presence.stopHubConnection();
    }

    register(model: any) {
        logForTrack('register(model:any)');
        return this.http.post(this.baseUrl + 'Account/register', model).pipe(
            map((res: User) => {
                if (res) {
                    this.setCurrentUser(res);
                    this.presence.createHubConnection(res);
                }
                return res;
            })
        )
    }

    getDecodedToken(token: string) {
        logForTrack('getDecodedToken(token: string)');
        return JSON.parse(atob(token.split('.')[1]));
    }
}
function logForTrack(funcName: string) {
    console.log(`account.service\n${funcName}}`);
    alert(`account.service\n${funcName}}`);
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
    var textZone = document.getElementById("modalText");
    textZone.innerHTML += `<br>account.service\n${funcName}}`;

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
