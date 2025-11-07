using Domain.Common;
using Domain.Common.Exceptions;
using Domain.Enums;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Agency : AuditableEntity
    {
        public string Name { get; private set; } = default!;
        public Email Email { get; private set; } = default!;
        public PhoneNumber PhoneNumber { get; private set; } = default!;
        public string? LogoUrl { get; private set; }
        public Address? Address { get; private set; }
        public Guid AgencyAdminId { get; private set; }
        public User AgencyAdmin { get; private set; } = default!;

        public ICollection<IncidentType> SupportedIncidents { get; private set; } = [];
        public ICollection<Responder> Responders { get; private set; } = [];

        private Agency() { }

        public Agency(Guid agencyAdminId, string name, Email email, PhoneNumber phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Agency name is required.");

            AgencyAdminId = agencyAdminId;
            Name = name;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        }

        public void ChangeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("Agency name is required.");

            Name = name;
            //AddDomainEvent(new AgencyNameChangedEvent(Id, Name));
        }

        public void ChangeContactInfo(Email email, PhoneNumber phoneNumber)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        }

        public void SetLogo(string logoUrl) => LogoUrl = logoUrl;
        public void SetAddress(Address address) => Address = address;

        public void AddSupportedIncident(IncidentType type)
        {
            if (SupportedIncidents.Contains(type))
                throw new BusinessRuleException($"Incident type '{type}' already supported.");

            SupportedIncidents.Add(type);
        }

        public void RemoveSupportedIncident(IncidentType type)
        {
            if (!SupportedIncidents.Contains(type))
                throw new InvalidOperationException($"Incident type '{type}' is not supported by this agency.");

            SupportedIncidents.Remove(type);
        }

    }
}