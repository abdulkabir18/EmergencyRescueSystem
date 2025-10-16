namespace Domain.Common.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, Guid id)
            : base($"{entityName} with Id '{id}' was not found.") { }
    }
}
