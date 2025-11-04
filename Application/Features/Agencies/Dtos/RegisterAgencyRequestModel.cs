using Application.Common.Dtos;
using Application.Features.Users.Dtos;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Application.Features.Agencies.Dtos
{
    public record RegisterAgencyRequestModel(
        RegisterUserRequestModel RegisterUserRequest,
        string AgencyName, string AgencyEmail, string AgencyPhoneNumber, IFormFile? AgencyLogo,
        AddressDto? AgencyAddress);

    public record IncidentTypesDto
    {
        public List<IncidentTypeDto> SupportedIncidents { get; set; }
    }

    public record RegisterAgencyFullRequestModel
    {
        public RegisterUserRequestModel RegisterUserRequest { get; set; }
        public string AgencyName { get; set; }
        public string AgencyEmail { get; set; }
        public string AgencyPhoneNumber { get; set; }
        public IFormFile? AgencyLogo { get; set; }
        public AddressDto? AgencyAddress { get; set; }

        public List<string> SupportedIncidents { get; set; }

        [JsonIgnore]
         public   List<IncidentType> IncidentTypesEnums =>
        ParseIncidentTypeList(SupportedIncidents);

        private List<IncidentType> ParseIncidentTypeList(List<string> source)
        {
            if (source == null) return new();

            var results = new List<IncidentType>();
            foreach (var s in source)
            {
                if (Enum.TryParse<IncidentType>(s, true, out var status))
                {
                    results.Add(status);
                }
                else
                {
                    throw new ArgumentException($"Invalid status: '{s}'");
                }
            }

            return results;
        }

    }
}