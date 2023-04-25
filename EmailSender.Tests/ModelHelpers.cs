using Bogus;
using MacroMail.Models;
using MacroMail.Models.Configuration;
using MacroMail.Models.Dto;

namespace MacroMail.Tests;

public static class ModelHelpers
{
    public static Faker<EmailMessage> EmailMessageFaker = new Faker<EmailMessage>()
                                                         .StrictMode(true)
                                                         .RuleFor(_ => _.Uid, f => Guid.NewGuid())
                                                         .RuleFor(_ => _.Sender, f => Guid.NewGuid())
                                                         .RuleFor(_ => _.Subject, f => f.Lorem.Text())
                                                         .RuleFor(_ => _.Content, f => f.Lorem.Paragraphs())
                                                         .RuleFor(_ => _.To, f => f.Person.Email);

    public static Faker<ContactRequest> ContactRequestFaker = new Faker<ContactRequest>()
                                                             .StrictMode(true)
                                                             .RuleFor(_ => _.Uid, f => f.Hashids.Encode())
                                                             .RuleFor(_ => _.Name, f => f.Person.FullName)
                                                             .RuleFor(_ => _.Email, f => f.Person.Email);

    public static Faker<SendingRequest> SendingRequestFaker = new Faker<SendingRequest>()
                                                             .StrictMode(true)
                                                             .RuleFor(_ => _.Sender, f => Guid.NewGuid())
                                                             .RuleFor(_ => _.Subject, f => f.Lorem.Text())
                                                             .RuleFor(_ => _.Content, f => f.Lorem.Paragraphs())
                                                             .RuleFor(_ => _.To, f => ContactRequestFaker.Generate())
                                                             .RuleFor(_ => _.Data,
                                                                      f => new Dictionary<string, object>());

    public static Faker<EmailConfiguration> EmailConfigurationFaker
        = new Faker<EmailConfiguration>()
         .StrictMode(true)
         .RuleFor(_ => _.Uid, f => Guid.NewGuid())
         .RuleFor(_ => _.Host,
                  f => f.Internet.DomainName())
         .RuleFor(_ => _.Port, f => f.Internet.Port())
         .RuleFor(_ => _.Email, f => f.Internet.Email())
         .RuleFor(_ => _.Password,
                  f => f.Internet.Password())
         .RuleFor(_ => _.AllowedHostSender, f => new[] { f.Internet.Ip(), f.Internet.Ip() });
}