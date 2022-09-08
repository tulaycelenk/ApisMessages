using Bridge.App.Core.Entity.Validation;
using DAL.Attributes;
using DCS.App.Service.Entity.Definition;
using System;

namespace DCS.App.Service.Entity {
    [Table("PSS.PASSENGER")]
    public class Passenger : DefinitionEntityDcs {
        public int FlightId { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PaxGenderCode { get; set; }
        [JoinColumn]
        public string PaxGenderValue { get; set; }
        public string Pnr { get; set; }
        public string EtkNumber { get; set; }
        public string Coupon { get; set; }
        public string PaxStatusCode { get; set; }
        [JoinColumn]
        public string PaxStatusValue { get; set; }
        public string SeatNumber { get; set; }
        [JoinColumn]
        public int BagCount { get; set; }
        [JoinColumn]
        public int BagTotalWeight { get; set; }
        public string SeatingClassCode { get; set; }
        public string BookingClassCode { get; set; }
        public string TicketingClassCode { get; set; }
        public string Fqtv { get; set; }
        public string InboundFlight { get; set; }
        public string OnwardFlight { get; set; }
        public string OnwardFlightTwo { get; set; }
        public string InfName { get; set; }
        public string InfSurname { get; set; }
        public DateTime? InfDob { get; set; }
        public string InfEtkNumber { get; set; }
        public string InfCoupon { get; set; }
        public string Comment { get; set; }
        public string GroupCode { get; set; }
        public string OffloadReasonCode { get; set; }
        public string Codeshare { get; set; }
        public string PaxDataSource { get; set; }

        public string FigureType { get; set; }
        [JoinColumn]
        public string OffloadReasonValue { get; set; }
        public string OffloadComment { get; set; }
        [JoinColumn]
        public bool HasInfant { get; set; }
        [JoinColumn]
        public bool HasApis { get; set; }
        [JoinColumn]
        public bool Has_Docs { get; set; }
        [JoinColumn]
        public bool Has_Doca { get; set; }
        [JoinColumn]
        public bool Has_Doca_Resi { get; set; }
        [JoinColumn]
        public bool Has_Doca_Dest { get; set; }
        [JoinColumn]
        public bool Has_Doco { get; set; }
        [JoinColumn]
        public bool Has_Hes { get; set; }
        public string GdsCode { get; set; }
        public string GdsPnr { get; set; }
        [JoinColumn]
        public string DocNumber { get; set; }
        [JoinColumn]
        public string DocIssuerNationalityCode { get; set; }
        public string CorporateGroup { get; set; }
        public int SequenceNumber { get; set; }
        public int SecurityNumber { get; set; }
        [JoinColumn]
        public bool HasOnwardFlight { get; set; }
        [JoinColumn]
        public string OnwardAdepAdesIata { get; set; }
        [JoinColumn]
        public string OnwardAdesIata { get; set; }
        [JoinColumn]
        public string InboundAdepAdesIata { get; set; }
        [JoinColumn]
        public string OnwardFlightNumber { get; set; }
        [JoinColumn]
        public string OnwardTwoAdepAdesIata{ get; set; }
        [JoinColumn]
        public string OnwardCityName { get; set; }
        [JoinColumn]
        public string Ssrs { get; set; }
        [JoinColumn]
        public string PassengerRecord { get; set; }
        [JoinColumn]
        public DateTime? DocsExpire { get; set; }
        [JoinColumn]
        public DateTime? InfDocsExpire { get; set; }
        [JoinColumn]
        public DateTime? DocoExpire { get; set; }
        [JoinColumn]
        public bool? DocoOk { get; set; }
        [JoinColumn]
        public bool? IsReserved { get; set; }
        public override ValidationResult Validate(ActionType actionType) {
            var collection = new ValidationMessageCollection();
            if (actionType != ActionType.Delete) {
                collection.ValidateRequiredField(Name, "Name");
                collection.ValidateRequiredField(Surname, "Surname");
                collection.ValidateRequiredField(PaxGenderCode, "PaxGenderCode");
                collection.ValidateRequiredField(PaxStatusCode, "PaxStatusCode");
            }
            return collection.ToValidationResult();
        }
    }
}
