using MeetingAppCore.DebugTracker;
using MeetingAppCore.Dtos;
using MeetingAppCore.Extensions;
using MeetingAppCore.Helpers;
using MeetingAppCore.Interfaces;
using MeetingAppCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Controllers
{
    [Authorize]
    public class MemberController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IHubContext<GroupHub> presenceHub;
        private readonly MeetingHub chatHub;
        private readonly PresenceTracker presenceTracker;

        public MemberController(IUnitOfWork unitOfWork, IHubContext<GroupHub> presenceHub, PresenceTracker presenceTracker)
        {
            this.unitOfWork = unitOfWork;
            this.presenceHub = presenceHub;
            this.presenceTracker = presenceTracker;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetAllMembers([FromQuery] UserParams userParams)
        {
            Console.WriteLine("1."+new String('=', 50));
            Console.WriteLine("1.Api/Member: GetAllMembers(UserParams)");
            FunctionTracker.Instance().AddApiFunc("Api/Acount: LoginSocial(LoginSocial)");
            userParams.CurrentUsername = User.GetUsername();
            var comments = await unitOfWork.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(comments.CurrentPage, comments.PageSize, comments.TotalCount, comments.TotalPages);

            return Ok(comments);
        }

        [HttpGet("{username}")] // member/username
        public async Task<ActionResult<MemberDto>> GetMember(string username)
        {
            Console.WriteLine("1."+new String('=', 50));
            Console.WriteLine("1.Api/Member: GetMembers(username)");
            FunctionTracker.Instance().AddApiFunc("1.Api/Member: GetMembers(username)");
            return Ok(await unitOfWork.UserRepository.GetMemberAsync(username));
        }

        [HttpPut("{username}")]
        public async Task<ActionResult> LockedUser(string username)
        {
            Console.WriteLine("1."+new String('=', 50));
            Console.WriteLine("1.Api/Member: LockedUser(username)");
            FunctionTracker.Instance().AddApiFunc("1.Api/Member: LockedUser(username)");
            var u = await unitOfWork.UserRepository.UpdateLocked(username);
            if(u != null)
            {
                var connections = await presenceTracker.GetConnectionsForUsername(username);
                await presenceHub.Clients.Clients(connections).SendAsync("OnLockedUser", true);
                return NoContent();
            }
            else
            {
                return BadRequest("Can not find given username");
            }
        }
    }
}
