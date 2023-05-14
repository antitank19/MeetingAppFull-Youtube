using AutoMapper;
using MeetingAppCore.Dtos;
using MeetingAppCore.Entities;
using MeetingAppCore.Extensions;
using MeetingAppCore.Helpers;
using MeetingAppCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeetingAppCore.Controllers
{
    [Authorize]
    public class RoomController : BaseApiController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public RoomController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAllRooms([FromQuery] RoomParams roomParams)
        {
            Console.WriteLine(new String('=', 10));
            Console.WriteLine("Api/Room: GetAllRooms(RoomParams)");
            var comments = await unitOfWork.RoomRepository.GetAllRoomAsync(roomParams);
            Response.AddPaginationHeader(comments.CurrentPage, comments.PageSize, comments.TotalCount, comments.TotalPages);

            return Ok(comments);
        }

        [HttpPost]
        public async Task<ActionResult> AddRoom(string name)
        {
            Console.WriteLine(new String('=', 10));
            Console.WriteLine("Api/Room: AddRoom(name)");
            var room = new Room { RoomName = name, UserId = User.GetUserId() };

            unitOfWork.RoomRepository.AddRoom(room);

            if (await unitOfWork.Complete())
            {
                return Ok(await unitOfWork.RoomRepository.GetRoomDtoById(room.RoomId));
            }

            return BadRequest("Problem adding room");
        }

        [HttpPut]
        public async Task<ActionResult> EditRoom(int id, string editName)
        {
            Console.WriteLine(new String('=', 10));
            Console.WriteLine("Api/Room: EditRoom(id, name)");
            var room = await unitOfWork.RoomRepository.EditRoom(id, editName);
            if(room != null)
            {
                if (unitOfWork.HasChanges())
                {
                    if (await unitOfWork.Complete())
                        return Ok(new RoomDto { RoomId = room.RoomId, RoomName = room.RoomName, UserId = room.UserId.ToString() });
                    return BadRequest("Problem edit room");
                }
                else
                {
                    return NoContent();
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoom(int id)
        {
            Console.WriteLine(new String('=', 10));
            Console.WriteLine("Api/Room: DeleteRoom(id)");
            var entity = await unitOfWork.RoomRepository.DeleteRoom(id);

            if(entity != null)
            {
                if (await unitOfWork.Complete())
                    return Ok(new RoomDto { RoomId = entity.RoomId, RoomName = entity.RoomName, UserId = entity.UserId.ToString() });
                return BadRequest("Problem delete room");
            }
            else
            {
                return NotFound();
            }
        }

        [HttpDelete("delete-all")]
        public async Task<ActionResult> DeleteAllRoom()
        {
            Console.WriteLine(new String('=', 10));
            Console.WriteLine("Api/Room: DeleteAllRoom()");
            await unitOfWork.RoomRepository.DeleteAllRoom();

            if (unitOfWork.HasChanges())
            {
                if (await unitOfWork.Complete())
                    return Ok();//xoa thanh cong
                return BadRequest("Problem delete all room");
            }
            else
            {
                return NoContent();//ko co gi de xoa
            }
        }
    }
}
