using System.Security.Claims;
using BlazorChat.Server.Application.Features.Messages;
using BlazorChat.Server.Application.Features.Messages.Commands;
using BlazorChat.Server.Controllers;
using BlazorChat.Shared.DTO;
using FluentAssertions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BlazorChat.Tests.Server.Controllers;

public class MessagesControllerTests
{
    [Fact]
    public async Task SendMessage_WhenMediatorReturnsForbidden_ShouldReturnForbidResult()
    {

        var mediatorMock = new Mock<IMediator>();
        var controller = new MessagesController(mediatorMock.Object);


        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "1")
        ]));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var dto = new SendMessageDto { ChannelId = 99, Content = "Test" };

        // Force Mediator to return a Forbidden result
        var failedResult = new MessageResult<MessageDto>(false, Error: MessageError.Forbidden);
        mediatorMock.Setup(m => m.Send(It.IsAny<SendMessageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResult);


        var response = await controller.SendMessage(dto, CancellationToken.None);



        response.Should().BeOfType<ForbidResult>();
    }
}