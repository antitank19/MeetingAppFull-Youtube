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
        private readonly IHubContext<PresenceHub> presenceHub;
        private readonly PresenceTracker presenceTracker;

        public MemberController(IUnitOfWork unitOfWork, IHubContext<PresenceHub> presenceHub, PresenceTracker presenceTracker)
        {
            this.unitOfWork = unitOfWork;
            this.presenceHub = presenceHub;
            this.presenceTracker = presenceTracker;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetAllMembers([FromQuery] UserParams userParams)
        {
            Console.WriteLine(new String('=', 10));
            Console.WriteLine("Api/Member: GetAllMembers(UserParams)");
            userParams.CurrentUsername = User.GetUsername();
            var comments = await unitOfWork.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(comments.CurrentPage, comments.PageSize, comments.TotalCount, comments.TotalPages);

            return Ok(comments);
        }

        [HttpGet("{username}")] // member/username
        public async Task<ActionResult<MemberDto>> GetMember(string username)
        {
            Console.WriteLine(new String('=', 10));
            Console.WriteLine("Api/Member: GetMembers(username)");
            return Ok(await unitOfWork.UserRepository.GetMemberAsync(username));
        }

        [HttpPut("{username}")]
        public async Task<ActionResult> LockedUser(string username)
        {
            Console.WriteLine(new String('=', 10));
            Console.WriteLine("Api/Member: LockedUser(username)");
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
