using Domain.Common;
using Domain.Common.Exceptions;
using Domain.Enums;
using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class Incident : AuditableEntity
    {
        public string? Title { get; private set; }
        public double? Confidence { get; private set; }
        public string ReferenceCode { get; private set; } = default!;
        public IncidentType Type { get; private set; }
        public IncidentStatus Status { get; private set; }
        public GeoLocation Coordinates { get; private set; } = default!;
        public Address? Address { get; private set; }
        public DateTime OccurredAt { get; private set; }

        public Guid UserId { get; private set; }
        public User User { get; private set; } = default!;

        public ICollection<Media> Medias { get; private set; } = [];
        public ICollection<IncidentResponder> AssignedResponders { get; private set; } = [];

        private Incident() { }

        public Incident(GeoLocation location, DateTime occurredAt, Guid userId)
        {
            Type = IncidentType.Unknown;
            Status = IncidentStatus.Pending;
            Coordinates = location;
            OccurredAt = occurredAt;
            UserId = userId;
            ReferenceCode = GenerateReferenceCode();

            AddDomainEvent(new IncidentCreatedEvent(Id, UserId, Type, Coordinates));
        }

        private static string GenerateReferenceCode()
        {
            var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpper();
            return $"INC-{datePart}-{randomPart}";
        }

        public void AddIncidentType(IncidentType type)
        {
            if (Type != IncidentType.Unknown)
                throw new InvalidOperationException("Incident type cannot be changed once set.");

            if (type == IncidentType.Unknown)
                throw new InvalidOperationException("Incident type can't be assigned to unknown");

            Type = type;
        }

        public void AssignResponder(Guid responderId, ResponderRole role)
        {
            if (Status is IncidentStatus.Resolved or IncidentStatus.Cancelled)
                throw new BusinessRuleException("Cannot assign responder to a resolved or cancelled incident.");

            AssignedResponders.Add(new IncidentResponder(this.Id, responderId, role));

            if (Status == IncidentStatus.Pending)
                Status = IncidentStatus.Reported;

            AddDomainEvent(new ResponderAssignedToIncidentEvent(Id, responderId, role));
        }

        public void MarkInProgress()
        {
            if (Status != IncidentStatus.Reported)
                throw new InvalidOperationException("Incident must be reported before it can be marked in progress.");

            Status = IncidentStatus.InProgress;
            AddDomainEvent(new IncidentStatusChangedEvent(Id, Status));
        }

        public void MarkAsReport()
        {
            if (Status == IncidentStatus.Pending)
                Status = IncidentStatus.Reported;
        }

        public void MarkResolved()
        {
            if (Status != IncidentStatus.InProgress)
                throw new InvalidOperationException("Incident must be in progress before it can be resolved.");

            Status = IncidentStatus.Resolved;
            AddDomainEvent(new IncidentStatusChangedEvent(Id, Status));
        }

        public void Escalate()
        {
            if (Status != IncidentStatus.InProgress)
                throw new InvalidOperationException("Incident must be in progress before it can be escalated.");

            Status = IncidentStatus.Escalated;
            AddDomainEvent(new IncidentStatusChangedEvent(Id, Status));
        }

        public void Cancel()
        {
            if (Status == IncidentStatus.Resolved)
                throw new InvalidOperationException("Cannot cancel a resolved incident.");

            Status = IncidentStatus.Cancelled;
            AddDomainEvent(new IncidentStatusChangedEvent(Id, Status));
        }

        public void AddMedia(string fileUrl, MediaType mediaType)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                throw new ArgumentException("File URL cannot be null or empty.", nameof(fileUrl));

            if (!Enum.IsDefined(typeof(MediaType), mediaType))
                throw new ArgumentException("Invalid media type.", nameof(mediaType));

            Medias.Add(new Media(fileUrl, mediaType));

            //AddDomainEvent(new IncidentMediaAddedEvent(Id, fileUrl, mediaType));
        }

        public void ApplyAiAnalysis(string? suggestedTitle, IncidentType detectedType, double confidence)
        {
            if (detectedType == IncidentType.Unknown || IsAiConfidenceLow(confidence, 0.7))
            {
                Title = suggestedTitle ?? "Unrecognized Media";
                Confidence = confidence;
                Status = IncidentStatus.Invalid;

                AddDomainEvent(new IncidentAnalyzedEvent(Id, IncidentType.Unknown, confidence, false));
                return;
            }

            Title = suggestedTitle;
            Confidence = confidence;

            if (Type == IncidentType.Unknown)
                Type = detectedType;

            Status = IncidentStatus.Analyzed;
            AddDomainEvent(new IncidentAnalyzedEvent(Id, detectedType, confidence, true));
        }

        public void SetAddress(Address address)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        public bool IsAiConfidenceLow(double threshold = 0.7) => Confidence.HasValue && Confidence.Value < threshold;
        private static bool IsAiConfidenceLow(double? confidence, double threshold) => !confidence.HasValue || confidence.Value < threshold;

    }
}