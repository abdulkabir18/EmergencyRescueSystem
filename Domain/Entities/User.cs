using Domain.Common;
using Domain.Common.Security;
using Domain.Common.Exceptions;
using Domain.Enums;
using Domain.Events;
using Domain.ValueObject;

namespace Domain.Entities
{
    public class User : AuditableEntity
    {
        public string FullName { get; private set; } = default!;
        public Email Email { get; private set; } = default!;
        public string? PasswordHash { get; private set; }
        public string? ProfilePictureUrl { get; private set; }
        public Gender Gender { get; private set; }
        public Address? Address { get; private set; }

        public bool IsEmailVerified { get; private set; }
        //public bool TwoFactorEnabled { get; private set; }

        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; }

        public Guid? AgencyId { get; private set; }
        public Guid? ResponderId { get; private set; }
        public Responder? Responder { get; private set; }
        public Agency? Agency { get; private set; } = default!;

        //public ICollection<EmergencyContact> EmergencyContacts { get; private set; } = [];
        public ICollection<Incident> Incidents { get; private set; } = [];
        //public ICollection<ChatParticipant> ChatParticipations { get; private set; } = [];
        //public ICollection<Message> Messages { get; private set; } = [];
        //public ICollection<IncidentLiveStreamParticipant> LiveStreamParticipations { get; private set; } = [];
        public ICollection<Notification> Notifications { get; private set; } = [];

        private User() { }

        public User(string fullName, Email email, Gender gender, UserRole role = UserRole.Victim)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ValidationException("Full name is required.");

            FullName = fullName;
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Gender = gender;

            Role = role;
            IsActive = true;
            IsEmailVerified = false;
            //TwoFactorEnabled = false;

            AddDomainEvent(new UserRegisteredEvent(Id, FullName, Email.Value, Role));
        }

        public static User RegisterWithGoogle(string fullName, Email email, string? profilePictureUrl, Gender gender, UserRole role = UserRole.Victim)
        {
            if (string.IsNullOrEmpty(fullName))
                throw new ValidationException("Full name is required.");

            var user = new User
            {
                FullName = fullName,
                Email = email,
                ProfilePictureUrl = profilePictureUrl,
                Gender = gender,
                Role = role,
                IsActive = true,
                IsEmailVerified = true,
                //TwoFactorEnabled = false
            };

            return user;
        }

        public void SetPassword(string rawPassword, IPasswordHasher hasher)
        {
            if (string.IsNullOrWhiteSpace(rawPassword))
                throw new ValidationException("Password cannot be empty.");

            PasswordHash = hasher.HashPassword(rawPassword);
        }

        public void ChangePassword(string newPassword, IPasswordHasher hasher)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ValidationException("Password cannot be empty.");

            PasswordHash = hasher.HashPassword(newPassword);
        }

        public bool VerifyPassword(string password, IPasswordHasher hasher)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("Password cannot be empty.");

            return hasher.VerifyPassword(PasswordHash!, password);
        }

        public void UpdateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ValidationException("Full name cannot be empty.");

            FullName = fullName;
        }

        public void SetAddress(Address address)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        public void SetProfilePicture(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ValidationException("Profile picture URL cannot be empty.");

            ProfilePictureUrl = imageUrl;
        }

        public void AssignToAgency(Guid agencyId) =>
            AgencyId = agencyId;

        public void AssignAsResponder(Guid responderId) =>
            ResponderId = responderId;

        //public void ChangeEmail(Email newEmail)
        //{
        //    if (Email == newEmail)
        //        throw new BusinessRuleException("New email cannot be the same as the old one.");

        //    Email = newEmail ?? throw new ArgumentNullException(nameof(newEmail));
        //    IsEmailVerified = false;

        //    AddDomainEvent(new UserEmailChangedEvent(Id, newEmail.Value));
        //}

        //public void ChangePhoneNumber(PhoneNumber newPhone)
        //{
        //    if (PhoneNumber == newPhone)
        //        throw new BusinessRuleException("New phone number cannot be the same as the old one.");

        //    PhoneNumber = newPhone ?? throw new ArgumentNullException(nameof(newPhone));
        //    IsPhoneNumberVerified = false;
        //}

        //public EmergencyContact AddEmergencyContact(string name, Email email, RelationshipType relationship, string? otherRelationship = null)
        //{
        //    var contact = new EmergencyContact(name, email, relationship, otherRelationship);

        //    if (EmergencyContacts.Any(c =>
        //        (c.PhoneNumber != null && c.PhoneNumber == contact.PhoneNumber) ||
        //        (c.Email != null && c.Email == contact.Email)))
        //    {
        //        throw new BusinessRuleException("An emergency contact with this phone or email already exists.");
        //    }

        //    EmergencyContacts.Add(contact);
        //    AddDomainEvent(new EmergencyContactAddedEvent(Id, contact.Name, contact.Email.Value, contact.GetRelationshipLabel()));
        //    return contact;
        //}

        //public void RemoveEmergencyContact(PhoneNumber? phoneNumber, Email? email)
        //{
        //    var contact = EmergencyContacts.FirstOrDefault(c =>
        //        (phoneNumber != null && c.PhoneNumber == phoneNumber) ||
        //        (email != null && c.Email == email));

        //    if (contact == null)
        //        throw new NotFoundException(nameof(EmergencyContact), phoneNumber?.Value != null ? Guid.Empty : Guid.Empty);

        //    EmergencyContacts.Remove(contact);
        //}

        // public void UpdateEmergencyContact(PhoneNumber? phoneNumber, Email? email, string? newName = null, RelationshipType? newRelationship = null, string? newOther = null)
        // {
        //     var contact = EmergencyContacts.FirstOrDefault(c =>
        //         (email != null && c.Email == email) ||
        //         (phoneNumber != null && c.PhoneNumber == phoneNumber));

        //     if (contact == null)
        //         throw new NotFoundException(nameof(EmergencyContact), Guid.Empty); // adjust key logic

        //     var updated = new EmergencyContact(
        //         newName ?? contact.Name,
        //         phoneNumber ?? contact.PhoneNumber!,
        //         email ?? contact.Email!,
        //         newRelationship ?? contact.Relationship,
        //         newOther ?? contact.OtherRelationship
        //     );

        //     if (EmergencyContacts.Any(c => c != contact &&
        //         ((c.PhoneNumber != null && c.PhoneNumber == updated.PhoneNumber) ||
        //          (c.Email != null && c.Email == updated.Email))))
        //     {
        //         throw new BusinessRuleException("Another emergency contact with this phone or email already exists.");
        //     }

        //     EmergencyContacts.Remove(contact);
        //     EmergencyContacts.Add(updated);
        // }

        public bool IsPasswordSet() => !string.IsNullOrEmpty(PasswordHash);

        public void VerifyEmail() => IsEmailVerified = true;

        //public void EnableTwoFactorAuth()
        //{
        //    TwoFactorEnabled = true;
        //}

        //public void DisableTwoFactorAuth()
        //{
        //    TwoFactorEnabled = false;
        //}

        public void Deactivate() => IsActive = false;
        public void Reactivate() => IsActive = true;
    }
}