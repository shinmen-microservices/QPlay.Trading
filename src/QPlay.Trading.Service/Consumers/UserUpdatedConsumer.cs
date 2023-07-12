using MassTransit;
using QPlay.Common.Repositories.Interfaces;
using QPlay.Identity.Contracts;
using QPlay.Trading.Service.Models.Entities;
using System.Threading.Tasks;

namespace QPlay.Trading.Service.Consumers;

public class UserUpdatedConsumer : IConsumer<UserUpdated>
{
    private readonly IRepository<ApplicationUser> repository;

    public UserUpdatedConsumer(IRepository<ApplicationUser> repository)
    {
        this.repository = repository;
    }

    public async Task Consume(ConsumeContext<UserUpdated> context)
    {
        UserUpdated message = context.Message;
        ApplicationUser user = await repository.GetAsync(message.UserId);

        if (user == null)
        {
            user = new ApplicationUser { Id = message.UserId, Gil = message.NewTotalGil };
            
            await repository.CreateAsync(user);
        }
        else
        {
            user.Gil = message.NewTotalGil;
            
            await repository.UpdateAsync(user);
        }
    }
}