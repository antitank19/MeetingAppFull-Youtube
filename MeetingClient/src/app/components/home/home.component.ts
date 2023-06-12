import { Component, ElementRef, HostListener, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TabDirective } from 'ngx-bootstrap/tabs';
import { Subscription } from 'rxjs';
import { eMeet } from 'src/app/models/eMeeting';
import { Member } from 'src/app/models/member';
import { Message } from 'src/app/models/message';
import { User } from 'src/app/models/user';
import { VideoElement } from 'src/app/models/video-element';
import { AccountService } from 'src/app/_services/account.service';
import { ChatHubService } from 'src/app/_services/chat-hub.service';
import { MessageCountStreamService } from 'src/app/_services/message-count-stream.service';
import Peer from "peerjs"; //tsconfig.json "esModuleInterop": true,
import { MuteCamMicService } from 'src/app/_services/mute-cam-mic.service';
import { RecordFileService } from 'src/app/_services/record-file.service';
import { ToastrService } from 'ngx-toastr';
import { ConfigService } from 'src/app/_services/ConfigService';
import { UtilityStreamService } from 'src/app/_services/utility-stream.service';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {
    isMeeting: boolean;
    messageInGroup: Message[] = [];
    currentRoomId = 0;
    currentUser: User;
    currentMember: Member;
    subscriptions = new Subscription();
    statusScreen: eMeet;
    chatForm: UntypedFormGroup;
    messageCount = 0;
    // shareScreenPeer: any;
    //shareScreenPeer: Peer;
    @ViewChild('videoPlayer') localvideoPlayer: ElementRef;
    shareScreenStream: any;
    enableShareScreen = true;// enable or disable button sharescreen
    isStopRecord = false;
    textStopRecord = 'Start Record';
    videos: VideoElement[] = [];
    isRecorded: boolean;
    userIsSharing: string;

    constructor(private chatHub: ChatHubService,
        private shareScreenService: MuteCamMicService,
        private configService: ConfigService,
        private route: ActivatedRoute,
        private toastr: ToastrService,
        private router: Router,
        private utility: UtilityStreamService,
        private recordFileService: RecordFileService,
        private messageCountService: MessageCountStreamService,
        private accountService: AccountService) {
        this.accountService.currentUser$.subscribe(user => {
            if (user) {
                this.currentUser = user;
                this.currentMember = { userName: user.userName, displayName: user.displayName } as Member
            }
        })
    }

    //chan khong cho tat trinh duyet khi o trang nay
    @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
        if (this.isMeeting) {
            $event.returnValue = true;
        }
    }

    roomId: string;
    // myRTCPeer: any;
    myRTCPeer: Peer;
    ngOnInit(): void {
        this.isMeeting = true
        this.isRecorded = this.configService.isRecorded;//enable or disable recorded
        const enableShareScreen = JSON.parse(localStorage.getItem('share-screen'))
        if (enableShareScreen) {// != null
            this.enableShareScreen = enableShareScreen
        }
        this.khoiTaoForm()
        this.roomId = this.route.snapshot.paramMap.get('id')
        this.createLocalStream()
        this.chatHub.createHubConnection(this.currentUser, this.roomId)

        logForTrack('myRTCPeer = new Peer(id: currentUserName, PeerOptions)');
        this.myRTCPeer = new Peer(this.currentUser.userName, {
            config: {
                'iceServers': [
                    {
                        urls: "stun:stun.l.google.com:19302",
                    },
                    {
                        urls: "turn:numb.viagenie.ca",
                        username: "webrtc@live.com",
                        credential: "muazkh"
                    },
                ],
            },
        },);

        this.myRTCPeer.on('open', userId => {
            logForTrack(`myRTCPeer.on('open', userId => {`);
            console.log(userId)
        });

        // this.shareScreenPeer = new Peer('share_' + this.currentUser.userName, {
        //     config: {
        //         'iceServers': [{
        //             urls: this.configService.STUN_SERVER
        //         }, {
        //             urls: this.configService.urlTurnServer,
        //             username: this.configService.username,
        //             credential: this.configService.password
        //         }]
        //     }
        // })

        // this.shareScreenPeer.on('call', (call) => {
        //     call.answer(this.shareScreenStream);
        //     call.on('stream', (otherUserVideoStream: MediaStream) => {
        //         this.shareScreenStream = otherUserVideoStream;
        //     });

        //     call.on('error', (err) => {
        //         console.error(err);
        //     })
        // });

        //call group
        this.myRTCPeer.on('call', (call) => {
            call.answer(this.stream);

            call.on('stream', (otherUserVideoStream: MediaStream) => {
                this.addOtherUserVideo(call.metadata.userId, otherUserVideoStream);
            });

            call.on('error', (err) => {
                console.error(err);
            })
        });

        this.subscriptions.add(
            this.chatHub.oneOnlineUser$.subscribe(member => {
                if (this.currentUser.userName !== member.userName) {
                    // Let some time for new peers to be able to answer
                    setTimeout(() => {
                        const call = this.myRTCPeer.call(member.userName, this.stream, {
                            metadata: { userId: this.currentMember },
                        });
                        call.on('stream', (otherUserVideoStream: MediaStream) => {
                            this.addOtherUserVideo(member, otherUserVideoStream);
                        });

                        call.on('close', () => {
                            this.videos = this.videos.filter((video) => video.user.userName !== member.userName);
                            //xoa user nao offline tren man hinh hien thi cua current user
                            this.tempvideos = this.tempvideos.filter(video => video.user.userName !== member.userName);
                        });
                    }, 1000);
                }
            })
        );

        this.subscriptions.add(this.chatHub.oneOfflineUser$.subscribe(member => {
            this.videos = this.videos.filter(video => video.user.userName !== member.userName);
            //xoa user nao offline tren man hinh hien thi current user
            this.tempvideos = this.tempvideos.filter(video => video.user.userName !== member.userName);
        }));

        this.subscriptions.add(
            this.chatHub.messagesThread$.subscribe(messages => {
                this.messageInGroup = messages;
            })
        );

        //hien thi so tin nhan chua doc
        this.subscriptions.add(
            this.messageCountService.messageCount$.subscribe(value => {
                this.messageCount = value;
            })
        );

        // bật chế độ share 1 màn hình lên, nhận từ chatHub
        this.subscriptions.add(
            this.shareScreenService.shareScreen$.subscribe(hasUserSharing => {
                //Nếu có người đang share screen thì làm 1 số cái như tắt nút shareScreen,
                // lưu local storage
                //Đổi status Screen để làm đó (tao BE đọc ko hiểu)
                if (hasUserSharing) {//true = share screen
                    this.statusScreen = eMeet.SHARESCREEN
                    this.enableShareScreen = false;// enable or disable button sharescreen
                    localStorage.setItem('share-screen', JSON.stringify(this.enableShareScreen));
                } else {// false = stop share
                    this.statusScreen = eMeet.NONE
                    this.enableShareScreen = true;
                    localStorage.setItem('share-screen', JSON.stringify(this.enableShareScreen));
                }
            })
        )

        // bắt đầu share stream tới user vao sau cùng từ user xuất phát stream
        // this.subscriptions.add(this.shareScreenService.lastShareScreen$.subscribe(val => {
        //     if (val.isShare) {//true = share screen        
        //         this.chatHub.shareScreenToUser(Number.parseInt(this.roomId), val.username, true)
        //         setTimeout(() => {
        //             const call = this.shareScreenPeer.call('share_' + val.username, this.shareScreenStream);
        //         }, 1000)
        //     }
        // }))

        this.subscriptions.add(this.utility.kickedOutUser$.subscribe(val => {
            this.isMeeting = false
            this.accountService.logout()
            this.toastr.info('You have been locked by admin')
            this.router.navigateByUrl('/login')
        }))

        this.subscriptions.add(this.shareScreenService.userIsSharing$.subscribe(val => {
            this.userIsSharing = val
        }))
    }

    //khong xai
    /* addMyVideo(stream: MediaStream) {
      this.videos.push({
        muted: true,
        srcObject: stream,
        user: { userName: this.currentUser.userName, displayName: this.currentUser.displayName } as Member,
      });
    } */

    addOtherUserVideo(userId: Member, stream: MediaStream) {
        const alreadyExisting = this.videos.some(video => video.user.userName === userId.userName);
        if (alreadyExisting) {
            console.log(this.videos, userId);
            return;
        }

        this.videos.push({
            muted: false,
            srcObject: stream,
            user: userId
        });

        if (this.videos.length <= this.maxUserDisplay) {
            this.tempvideos.push({
                muted: false,
                srcObject: stream,
                user: userId
            })
        }
    }

    maxUserDisplay = 8; // chi hien toi da la 8 user
    tempvideos: VideoElement[] = [];

    stream: any;
    enableVideo = true;
    enableAudio = true;

    async createLocalStream() {
        try {
            this.stream = await navigator.mediaDevices.getUserMedia({ video: this.enableVideo, audio: this.enableAudio });
            this.localvideoPlayer.nativeElement.srcObject = this.stream;
            this.localvideoPlayer.nativeElement.load();
            this.localvideoPlayer.nativeElement.play();
        } catch (error) {
            console.error(error);
            //alert(`Can't join room, error ${error}`);
        }
    }

    enableOrDisableVideo() {
        this.enableVideo = !this.enableVideo
        if (this.stream.getVideoTracks()[0]) {
            this.stream.getVideoTracks()[0].enabled = this.enableVideo;
            this.chatHub.muteCamera(this.enableVideo)
        }
    }

    enableOrDisableAudio() {
        this.enableAudio = !this.enableAudio;
        if (this.stream.getAudioTracks()[0]) {
            this.stream.getAudioTracks()[0].enabled = this.enableAudio;
            this.chatHub.muteMicroPhone(this.enableAudio)
        }
    }

    onSelect(data: TabDirective): void {
        if (data.heading == "Chat") {
            this.messageCountService.ActiveTabChat = true;
            this.messageCountService.MessageCount = 0;
            this.messageCount = 0;
        } else {
            this.messageCountService.ActiveTabChat = false;
        }
    }

    khoiTaoForm() {
        this.chatForm = new UntypedFormGroup({
            content: new UntypedFormControl('', Validators.required)
        })
    }

    sendMessage() {
        this.chatHub.sendMessage(this.chatForm.value.content).then(() => {
            this.chatForm.reset();
        })
    }

    async shareScreen() {
        try {
            // @ts-ignore
            let mediaStream = await navigator.mediaDevices.getDisplayMedia({ video: true });
            this.chatHub.shareScreen(Number.parseInt(this.roomId), true);
            this.shareScreenStream = mediaStream;
            this.enableShareScreen = false;

            this.videos.forEach(v => {
                // const call = this.shareScreenPeer.call('share_' + v.user.userName, mediaStream);
                //call.on('stream', (otherUserVideoStream: MediaStream) => { });
            })

            mediaStream.getVideoTracks()[0].addEventListener('ended', () => {
                this.chatHub.shareScreen(Number.parseInt(this.roomId), false);
                this.enableShareScreen = true;
                localStorage.setItem('share-screen', JSON.stringify(this.enableShareScreen));
            });
        } catch (e) {
            console.log(e);
            alert(e)
        }
    }

    StartRecord() {
        this.isStopRecord = !this.isStopRecord;
        if (this.isStopRecord) {
            this.textStopRecord = 'Stop record';
            this.recordFileService.startRecording(this.stream);
        } else {
            this.textStopRecord = 'Start record';
            this.recordFileService.stopRecording();
            setTimeout(() => {
                this.recordFileService.upLoadOnServer().subscribe(() => {
                    this.toastr.success('Upload file on server success');
                })
            }, 1000)
        }
    }

    /*   getTURNServer(): any{
        return { 'iceServers': [
          { url:'stun:stun.12voip.com:3478'}
       ] };
      } */

    ngOnDestroy() {
        this.isMeeting = false;

        this.myRTCPeer.disconnect();//dong ket noi nhung van giu nguyen cac ket noi khac
        // this.shareScreenPeer.destroy();//dong tat ca cac ket noi
        this.chatHub.stopHubConnection();
        this.subscriptions.unsubscribe();
        localStorage.removeItem('share-screen');
    }

    onLoadedMetadata(event: Event) {
        (event.target as HTMLVideoElement).play();
    }
}
function logForTrack(funcName: string) {
    console.log(`home.component\n${funcName}}`);
    //alert(`home.component\n${funcName}}`);
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
    text.innerHTML += `<br>home.component\n${funcName}}`;

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