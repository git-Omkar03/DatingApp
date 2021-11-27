using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly IMessageRepository messageRepository;

        public MessagesController(IMapper mapper, IUserRepository userRepository, IMessageRepository messageRepository)
        {
            this.mapper = mapper;
            this.userRepository = userRepository;
            this.messageRepository = messageRepository;
        }

        [HttpPost]

        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var UserName = User.GetUserName();

            if (UserName == createMessageDto.RecipientUserName.ToLower())
                return BadRequest("You cannot send messages to yourself");

            var sender = await userRepository.GetUserByUserNameAsync(UserName);

            var recipient = await userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUserName = sender.UserName,
                ReceipientUserName = recipient.UserName,
                Content = createMessageDto.Content
            };

            messageRepository.AddMessage(message);

            if (await messageRepository.SaveAllAsync()) return Ok(
                mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send Messsage");
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.GetUserName();
            var messages = await messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.Pagesize,
                messages.TotalCount, messages.TotalPages);

            return Ok(messages);
        }

        [HttpGet("thread/{UserName}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string UserName)
        {
            var currentUserName = User.GetUserName();

            return Ok(await messageRepository.GetMessageThread(currentUserName, UserName));
        }

        [HttpDelete("{id}")]

        public async Task<ActionResult> DeleteMessage(int id)
        {
            var UserName = User.GetUserName();

            var message = await messageRepository.GetMessage(id);

            if (message.Sender.UserName != UserName && message.Recipient.UserName != UserName)
                return Unauthorized();

            if (message.Sender.UserName == UserName) message.SenderDeleted = true;

            if (message.Recipient.UserName == UserName) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                messageRepository.DeleteMessage(message);

            if (await messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem Deleting Message");
        }
    }
}
