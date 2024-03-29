import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { User } from './models/user';
import { AccountService } from './_services/account.service';
import { PresenceHubService } from './_services/presence-hub.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  title = 'MeetingClient';

  constructor(private router: Router, private accountService: AccountService, private presence: PresenceHubService){}

  ngOnInit(): void {
    this.setCurrentUser();
  }

  setCurrentUser(){
    const user: User = JSON.parse(localStorage.getItem('user'));
    if(user){
      this.accountService.setCurrentUser(user);
      this.presence.createHubConnection(user);
    }else{
      this.router.navigateByUrl('login');
    }    
  }
  
}
