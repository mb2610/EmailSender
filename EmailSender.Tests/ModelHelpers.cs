using Bogus;
using MacroMail.Models;
using MacroMail.Models.Configuration;
using MacroMail.Models.Dto;

namespace MacroMail.Tests;

public static class ModelHelpers
{
    public static readonly Faker<EmailMessage> EmailMessageFaker = new Faker<EmailMessage>()
                                                                  .StrictMode(false)
                                                                  .RuleFor(_ => _.Uid, f => Guid.NewGuid())
                                                                  .RuleFor(_ => _.Sender, f => Guid.NewGuid())
                                                                  .RuleFor(_ => _.Subject, f => f.Lorem.Text())
                                                                  .RuleFor(_ => _.Content, f => f.Lorem.Paragraphs())
                                                                  .RuleFor(_ => _.To, f => f.Person.Email);

    public static readonly Faker<ContactRequest> ContactRequestFaker = new Faker<ContactRequest>()
                                                                      .StrictMode(false)
                                                                      .RuleFor(_ => _.Uid, f => f.Hashids.Encode())
                                                                      .RuleFor(_ => _.Name, f => f.Person.FullName)
                                                                      .RuleFor(_ => _.Email, f => f.Person.Email);

    public static readonly Faker<SendingRequest> SendingRequestFaker = new Faker<SendingRequest>()
                                                                     .StrictMode(false)
                                                                     .RuleFor(_ => _.Sender, f => Guid.NewGuid())
                                                                     .RuleFor(_ => _.Subject, f => f.Lorem.Text())
                                                                     .RuleFor(_ => _.Content, f => f.Lorem.Paragraphs())
                                                                     .RuleFor(_ => _.To, f => ContactRequestFaker.Generate())
                                                                     .RuleFor(_ => _.Data,
                                                                              f => new Dictionary<string, object>());

    public static readonly Faker<EmailConfiguration> EmailConfigurationFaker
        = new Faker<EmailConfiguration>()
         .StrictMode(false)
         .RuleFor(_ => _.Uid, f => Guid.NewGuid())
         .RuleFor(_ => _.Host,
                  f => f.Internet.DomainName())
         .RuleFor(_ => _.Port, f => f.Internet.Port())
         .RuleFor(_ => _.Email, f => f.Internet.Email())
         .RuleFor(_ => _.Password,
                  f => f.Internet.Password())
         .RuleFor(_ => _.AllowedHostSender, f => new[] { f.Internet.Ip(), f.Internet.Ip() });
}