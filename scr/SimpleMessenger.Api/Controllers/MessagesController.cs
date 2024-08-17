﻿using FluentValidation;
using Infrastucture.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SimpleMessenger.Api.Hubs;
using SimpleMessenger.Contracts.Dto;
using SimpleMessenger.Contracts.Requests;
using System;

namespace SimpleMessenger.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessagesController(
    IValidator<MessageDto> _validator,
    IMessagesService _service,
    IHubContext<MessageHub> _hubContext) : ControllerBase
{
    /// <summary>
    /// Получить сообщения отправленные после определенного времени
    /// </summary>
    /// <param name="minutes"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<ICollection<MessageDto>>> GetMessagesSentAfterAsync([FromQuery(Name = "sent-after")] DateTime sentAfter)
    {
        var messageDtos = await _service.GetMessagesSentAfterAsync(sentAfter);

        return Ok(messageDtos);
    }

    /// <summary>
    /// Отправить сообщение
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageRequest request)
    {
        var messageDto = new MessageDto(
                request.Content,
                DateTime.Now,
                request.SequenceNumber
                );

        var validationResult = await _validator.ValidateAsync(messageDto);

        if (!validationResult.IsValid)
        {
            validationResult.Errors.ForEach(
                error => ModelState.AddModelError(error.PropertyName, error.ErrorMessage)
                );

            return BadRequest(ModelState);
        }

        await _service.CreateMessageAsync(messageDto);

        await _hubContext.Clients.All.SendAsync("receiveMessage", messageDto);

        return Ok();
    }
}
