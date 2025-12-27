using Mapster;
using TransactSmartFilter.Application.Dtos.Responses;
using TransactSmartFilter.Domain.Models;

namespace TransactSmartFilter.Application.Mapper;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Transaction, TransactionResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.AccountId, src => src.AccountId)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.TransactionTypeId, src => src.TransactionTypeId)
            .Map(dest => dest.TransactionStatusId, src => src.TransactionStatusId)
            .Map(dest => dest.PaymentMethodId, src => src.PaymentMethodId)
            .Map(dest => dest.RecipientName, src => src.RecipientName)
            .Map(dest => dest.RecipientEmail, src => src.RecipientEmail)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.CreatedAtUtc, src => src.CreatedAt)
            .Map(dest => dest.Tags, src =>
                src.TransactionTags != null
                    ? src.TransactionTags.Select(tt => tt.Tag.Name).ToList()
                    : new List<string>()
            );
    }
}