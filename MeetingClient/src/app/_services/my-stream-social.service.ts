import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { FacebookLoginProvider, GoogleLoginProvider, SocialAuthService, SocialUser } from '@abacritt/angularx-social-login';
import { AccountService } from './account.service';

@Injectable({
    providedIn: 'root'
})
export class MyStreamSocialService {
    private socialUser: SocialUser;

    constructor(private authService: SocialAuthService, private router: Router, private accountService: AccountService) {
        this.authService.authState.subscribe((user) => {
            this.socialUser = user;
            if (this.socialUser) {
                let model = {
                    provider: this.socialUser.provider,
                    email: this.socialUser.email,
                    name: this.socialUser.name,
                    photoUrl: this.socialUser.photoUrl
                }

                this.accountService.loginWithSocial(model).subscribe(res => {
                    this.router.navigateByUrl('/room');
                })
            }
        });
    }

    set UserSocial(value: SocialUser) {
        this.socialUser = value;
    }

    get UserSocial(): SocialUser {
        return this.socialUser;
    }

    signOutSocial(): void {
        this.authService.signOut();
    }

    // signInWithGoogle(): void {
    //   this.authService.signIn(GoogleLoginProvider.PROVIDER_ID);
    // }

    signInWithFacebook(): void {
        this.authService.signIn(FacebookLoginProvider.PROVIDER_ID);
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