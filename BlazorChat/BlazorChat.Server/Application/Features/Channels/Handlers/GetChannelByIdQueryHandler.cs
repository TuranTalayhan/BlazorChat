using BlazorChat.Server.Application.Features.Channels.Queries;
using BlazorChat.Server.Application.Interfaces.Repositories;
using BlazorChat.Shared.DTO;
using Mediator;

namespace BlazorChat.Server.Application.Features.Channels.Handlers;

public class GetChannelByIdQueryHandler(IChannelRepository channelRepository) 
    : IQueryHandler<GetChannelByIdQuery, ChannelResult<ChannelDto>>
{
    public async ValueTask<ChannelResult<ChannelDto>> Handle(GetChannelByIdQuery request, CancellationToken ct)
    {
        var dto = await channelRepository.GetChannelWithDetailsAsync(request.ChannelId, ct);

        if (dto == null)
        {
            return new ChannelResult<ChannelDto>(false, Error: ChannelError.NotFound);
        }

        switch (dto.Type)
        {
            case ChannelType.Server:
            {
                var isServerMember = await channelRepository.IsServerMemberAsync(dto.ServerId.GetValueOrDefault(), request.CurrentUserId, ct);
                if (!isServerMember)
                {
                    return new ChannelResult<ChannelDto>(false, Error: ChannelError.Forbidden);
                }

                break;
            }
            case ChannelType.DirectMessage:
            {
                var isDmMember = dto.Members.Any(m => m.Id == request.CurrentUserId);
                if (!isDmMember)
                {
                    return new ChannelResult<ChannelDto>(false, Error: ChannelError.Forbidden);
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        return new ChannelResult<ChannelDto>(true, Data: dto);
    }
}