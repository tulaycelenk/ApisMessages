using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DCS.App.Service.Entity.Definition;
using Bridge.App.Core.Entity.Validation;
using DAL.Attributes;
using Bridge.App.Core.Entity;

namespace DCS.App.Service.Entity
{
    public class PassengerDocs : ModifiableEntity, IEntity
    {
        [Key(IsIdentity = true)]
        [Column("ID")]
        [OrderBy]
        public int Id { get; set; }
        public int PassengerId { get; set; }
        public string DocTypeCode { get; set; }
        public string DocNumber { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string GenderCode { get; set; }
        public DateTime? Dob { get; set; }
        public string NationalityCode { get; set; }
        public string DocIssuerNationalityCode { get; set; }
        public DateTime? Doe { get; set; }
        public string NationalId { get; set; }
        public string CustomDocTypeCode { get; set; }
        public string CustomDocTypeName { get; set; }
        public string InfDocTypeCode { get; set; }
        public string InfDocNumber { get; set; }
        public string InfSurname { get; set; }
        public string InfName { get; set; }
        public string InfGenderCode { get; set; }
        public DateTime? InfDob { get; set; }
        public string InfNationalityCode { get; set; }
        public string InfDocIssuerNationalityCode { get; set; }
        public DateTime? InfDoe { get; set; }
        public string InfNationalId { get; set; }
        public string InfCustomDocTypeCode { get; set; }
        public string InfCustomDocTypeName { get; set; }
        [JoinColumn]
        public string PassengerStatusCode { get; set; }
        [JoinColumn]
        public string InboundFlightIata { get; set; }
        [JoinColumn]
        public string OnwardFlightIata { get; set; }
        [JoinColumn]
        public string PassengerPnr { get; set; }
        [JoinColumn]
        public string PassengerSeatNumber { get; set; }
        [JoinColumn]
        public int FlightId { get; set; }
        [JoinColumn]
        public bool IsDocsLimit { get; set; }
        [JoinColumn]
        public int DocsUpperLimit { get; set; }
        [JoinColumn]
        public int DocsLowerLimit { get; set; }
        [JoinColumn]
        public bool InfIsDocsLimit { get; set; }
        [JoinColumn]
        public int InfDocsUpperLimit { get; set; }
        [JoinColumn]
        public int InfDocsLowerLimit { get; set; }
        [JoinColumn]
        public bool? HasInfant { get; set; }

        public override ValidationResult Validate(ActionType actionType)
        {
            var collection = new ValidationMessageCollection();
            if (actionType != ActionType.Delete)
            {
                collection.ValidateRequiredField(PassengerId, "Passenger");
            }
            return collection.ToValidationResult();
        }

    }
}

