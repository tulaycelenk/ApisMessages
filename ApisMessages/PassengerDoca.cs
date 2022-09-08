using Bridge.App.Core.Entity;
using Bridge.App.Core.Entity.Validation;
using DAL.Attributes;

namespace DCS.App.Service.Entity.Dcs
{
    [Table("PSS.PASSENGER_DOCA")]
    public class PassengerDoca : ModifiableEntity, IEntity
    {
        [Key(IsIdentity = true)]
        [Column("ID")]
        [OrderBy]
        public int Id { get; set; }
        public int PassengerId { get; set; }
        public int? DestinationCountryId { get; set; }
        public string DestinationCity { get; set; }
        public string DestinationAddress { get; set; }
        public string DestinationStateProvince { get; set; }
        public string DestinationZipCode { get; set; }
        public int? ResidenceCountryId { get; set; }
        public string ResidenceCity { get; set; }
        public string ResidenceAddress { get; set; }
        public string ResidenceStateProvince { get; set; }
        public string ResidenceZipCode { get; set; }
        [JoinColumn]
        public string DestinationCountryIsoCode { get; set; }
        [JoinColumn]
        public string DestinationCountryIso3Code { get; set; }
        [JoinColumn]
        public bool IsDocaDest { get; set; }
        [JoinColumn]
        public string ResidenceCountryIsoCode { get; set; }
        [JoinColumn]
        public string ResidenceCountryIso3Code { get; set; }
        [JoinColumn]
        public bool IsDocaResi { get; set; }

        public override ValidationResult Validate(ActionType actionType) {
            var collection = new ValidationMessageCollection();
            if (actionType != ActionType.Delete) {
                collection.ValidateRequiredField(PassengerId, "Passenger");
            }
            return collection.ToValidationResult();
        }

    }
}
