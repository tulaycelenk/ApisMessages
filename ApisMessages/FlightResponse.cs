using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


#region Assembly AviatorManager.ViewModel, Version=3.3.0.39978, Culture=neutral, PublicKeyToken=null
// C:\BTFS\AODB\DEV\CommonBin\InternalBin\AviatorManager.ViewModel.dll
#endregion

using System;

namespace AviatorManager.ViewModel.Aodb
{
    public class FlightResponse
    {
        public FlightResponse();

        public string FormattedPairFlightNumber { get; set; }
        public string RunwayCode { get; set; }
        public string StandCode { get; set; }
        public int? GateId { get; set; }
        public string ChuteCode { get; set; }
        public string GateCode { get; set; }
        public string FormattedFlightNumber { get; set; }
        public string AircraftTypeIcao { get; set; }
        public string AircraftTypeIata { get; set; }
        public string RegNo { get; set; }
        public string AircraftRegNo { get; set; }
        public string DepartureIcao { get; set; }
        public int? EstimatedTaxiInTime { get; set; }
        public string DepartureIata { get; set; }
        public string DestinationIata { get; set; }
        public string AcWakeCategoryCode { get; set; }
        public string AcWakeCategoryName { get; set; }
        public string FlightServiceTypeCode { get; set; }
        public string FlightservicetypeName { get; set; }
        public string AircrafttypeName { get; set; }
        public string CountryIsoCode { get; set; }
        public string DepartureCountryIsoCode { get; set; }
        public string DestinationCountryIsoCode { get; set; }
        public string DestinationName { get; set; }
        public string DepartureName { get; set; }
        public string BaggageWeightPreference { get; set; }
        public string DestinationIcao { get; set; }
        public string AirlineIataCode { get; set; }
        public string CdmStatus { get; set; }
        public string DcsStatus { get; set; }
        public virtual bool IsActive { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatingTranCode { get; set; }
        public string UpdatingUserCode { get; set; }
        public string Icons { get; set; }
        public string GroundHandler { get; set; }
        public string Ssr { get; set; }
        public bool DeicingRequest { get; set; }
        public bool Override { get; set; }
        public int AdesPlanningPriority { get; set; }
        public int AdepPlanningPriority { get; set; }
        public int Axot { get; set; }
        public string AirportStatus { get; set; }
        public int Axit { get; set; }
        public int Exit { get; set; }
        public string CountryName { get; set; }
        public string DmanStatus { get; set; }
        public string SpdsStatus { get; set; }
        public string AsmgcsStatus { get; set; }
        public string AtcStatus { get; set; }
        public string AftnStatus { get; set; }
        public string BrsStatus { get; set; }
        public string FidslandsideStatus { get; set; }
        public string FidsairsideStatus { get; set; }
        public string CmsStatus { get; set; }
        public string WbStatus { get; set; }
        public int Exot { get; set; }
        public string AirlineIcaoCode { get; set; }
        public string AirlineName { get; set; }
        public bool FlpActive { get; set; }
        public DateTime? Acgt { get; set; }
        public DateTime? Asbt { get; set; }
        public DateTime? Tobt { get; set; }
        public DateTime? Cobt { get; set; }
        public DateTime? Aobt { get; set; }
        public DateTime? Eobt { get; set; }
        public DateTime? Sobt { get; set; }
        public string GroundHandlerCode { get; set; }
        public int AdesPairId { get; set; }
        public int AdepPairId { get; set; }
        public string FlightRule { get; set; }
        public int FlightServiceTypeId { get; set; }
        public DateTime? Tsat { get; set; }
        public int FlightEet { get; set; }
        public int AcTypeId { get; set; }
        public string Star { get; set; }
        public int AdesId { get; set; }
        public DateTime? AdesFlightDate { get; set; }
        public string Sid { get; set; }
        public int AdepId { get; set; }
        public DateTime? AdepFlightDate { get; set; }
        public string CallSign { get; set; }
        public string FlightSuffix { get; set; }
        public int FlightNumber { get; set; }
        public int AirlineId { get; set; }
        public int Id { get; set; }
        public int AircraftId { get; set; }
        public DateTime? Ardt { get; set; }
        public DateTime? Asrt { get; set; }
        public DateTime? Asat { get; set; }
        public int TobtCount { get; set; }
        public string SourceUniqueId { get; set; }
        public string UniqueFlightCode { get; set; }
        public int AdesProjectId { get; set; }
        public int AdepProjectId { get; set; }
        public bool IsCodeshare { get; set; }
        public string FlightType { get; set; }
        public int FlightSourceCode { get; set; }
        public DateTime? Esbt { get; set; }
        public DateTime? Cibt { get; set; }
        public DateTime? Aibt { get; set; }
        public DateTime? Eibt { get; set; }
        public DateTime? Sibt { get; set; }
        public DateTime? Aldt { get; set; }
        public DateTime? Cldt { get; set; }
        public DateTime? Eldt { get; set; }
        public DateTime? Sldt { get; set; }
        public DateTime? Atot { get; set; }
        public DateTime? Ttot { get; set; }
        public DateTime? Etot { get; set; }
        public DateTime? Ctot { get; set; }
        public DateTime? Aezt { get; set; }
        public DateTime? Eezt { get; set; }
        public DateTime? Aczt { get; set; }
        public DateTime? Eczt { get; set; }
        public DateTime? Arzt { get; set; }
        public DateTime? Erzt { get; set; }
        public string DepartureCityName { get; set; }
        public string DestinationCityName { get; set; }
    }
}

