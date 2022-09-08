using System;
using DCS.App.Service.Entity.Definition;
using Bridge.App.Core.Entity.Validation;
using DAL.Attributes;
using Bridge.App.Core.Entity;

namespace DCS.App.Service.Entity {
    [Table("PSS.PASSENGER_DOCO")]
    public class PassengerDoco : ModifiableEntity, IEntity {
        [Key(IsIdentity = true)]
        [Column("ID")]
        [OrderBy]
        public int Id { get; set; }
        public int PassengerId { get; set; }
        public string DocTypeCode { get; set; }
        [JoinColumn]
        public string DocTypeValue { get; set; }
        public string DocNumber { get; set; }
        public string DocForNationalityCode { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string GenderCode { get; set; }
        [JoinColumn]
        public string GenderValue { get; set; }
        public DateTime? Dob { get; set; }
        public string NationalityCode { get; set; }
        [JoinColumn]
        public string NationalityName { get; set; }
        public DateTime? Doe { get; set; }
        public string CustomDocTypeCode { get; set; }
        public string CustomDocTypeName { get; set; }
        public bool DocoOk { get; set; }
        [JoinColumn]
        public bool IsDocoLimit { get; set; }
        [JoinColumn]
        public int DocoUpperLimit { get; set; }
        [JoinColumn]
        public int DocoLowerLimit { get; set; }

        public override ValidationResult Validate(ActionType actionType) {
            var collection = new ValidationMessageCollection();
            if (actionType != ActionType.Delete) {
                collection.ValidateRequiredField(PassengerId, "Passenger");                
            }
            return collection.ToValidationResult();
        }

    }
}
