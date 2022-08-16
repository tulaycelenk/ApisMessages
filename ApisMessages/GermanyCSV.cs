using AviatorManager.ViewModel.Aodb;
using Bridge.App.Core.Exceptions;
using Bridge.Common.Logging;
using DCS.App.Service.Entity;
using DCS.App.Service.Resources;
using DCS.ViewModel.Paging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

    namespace ApisMessages
    {
    class GermanyCsvPaxLstExporter : IExporter {
        private readonly CultureInfo enUs = new CultureInfo("en-Us");
        /// <summary>
        /// Germany Csv PaxLst
        /// </summary>
        /// <param name="flight"></param>
        /// <param name="singleOrMultiPaxLst"></param>
        /// <returns></returns>
        public byte[] Export(FlightResponse flight, string exporterType, bool singleOrMultiPaxLst) {
            IEnumerable<PassengerDocs> passengerDocsList = PassengerDocsService.GetPassengerWithDocsInformation(new PassengerDocsPagingRequest { FlightId = flight.Id, PageNumber = 1, PageSize = 2000}).Item1;
            passengerDocsList = passengerDocsList.Where(p => p.PassengerStatusCode == Bridge.Common.ParameterHelper.PaxStatusCode.Flown || p.PassengerStatusCode == Bridge.Common.ParameterHelper.PaxStatusCode.Boarded || p.PassengerStatusCode == Bridge.Common.ParameterHelper.PaxStatusCode.CheckedIn);

            using (var ms = new MemoryStream()) {
                TextWriter tw = new StreamWriter(ms);
                var enUs = new CultureInfo("en-US");
                string docsGenderCode = string.Empty;
                var sobt = flight.Sobt.HasValue ? flight.Sobt.Value.ToString("yyMMddHHmm", enUs) : "";
                var sldt = flight.Sldt.HasValue ? flight.Sldt.Value.ToString("yyMMddHHmm", enUs) : "";
                var flightNumber = flight.AirlineIataCode + flight.FlightNumber;

                tw.WriteLine(flight.AirlineIataCode + ";" + flightNumber + ";" + flight.DepartureIata + ";" + sobt + ";" + flight.DestinationIata + ";" + sldt + ";" + passengerDocsList.Count());

                foreach (var passengerDocs in passengerDocsList) {
                    string docsSurname = !string.IsNullOrEmpty(passengerDocs.Surname) ? (passengerDocs.Surname.Length > 35 ? passengerDocs.Surname.Substring(0, 35) : passengerDocs.Surname) : "";
                    string docsName = !string.IsNullOrEmpty(passengerDocs.Name) ? (passengerDocs.Name.Length > 35 ? passengerDocs.Name.Substring(0, 35) : passengerDocs.Name) : "";
                    if (!string.IsNullOrEmpty(passengerDocs.GenderCode)) {
                        if (passengerDocs.GenderCode == "FC")
                            docsGenderCode = "F";
                        else if (passengerDocs.GenderCode == "MC")
                            docsGenderCode = "M";
                        else if (passengerDocs.GenderCode == "C")
                            docsGenderCode = "U";
                        else
                            docsGenderCode = passengerDocs.GenderCode;
                    }
                    var docsDob = passengerDocs.Dob.HasValue ? passengerDocs.Dob.Value.ToString("yyMMdd") : "YY0000";
                    string docsNationalityCode = passengerDocs.NationalityCode == "DEU" ? "D" : passengerDocs.NationalityCode;
                    string docsInboundFlightIata = passengerDocs.InboundFlightIata != null ? passengerDocs.InboundFlightIata : flight.DepartureIata;
                    string docsOnwardFlightIata = passengerDocs.OnwardFlightIata != null ? passengerDocs.OnwardFlightIata : flight.DestinationIata;
                    string docsIssuerNationalityCode = passengerDocs.DocIssuerNationalityCode == "DEU" ? "D" : passengerDocs.DocIssuerNationalityCode;
                    string docsTypeCode = passengerDocs.DocTypeCode == "O" ? passengerDocs.CustomDocTypeCode : passengerDocs.DocTypeCode;
                    var docsDocNumber = passengerDocs.DocNumber;

                    var passengerDoco = PassengerDocoService.GetPassengerDoco(passengerDocs.PassengerId).FirstOrDefault(); ;
                    var docoDocForNatioanalityCode = passengerDoco != null  ? (passengerDoco.DocForNationalityCode == "DEU" ? "D" : passengerDoco.DocForNationalityCode) : "";
                    var docoDocType = passengerDoco != null  ? ((passengerDoco.DocForNationalityCode != null || passengerDoco.DocNumber !=null  || passengerDoco.Name != null || passengerDoco.Surname != null)  ? "V" :""): "";
                    var docoDocNumber = passengerDoco != null ? passengerDoco.DocNumber : "";

                    tw.WriteLine(docsSurname + ";" + docsName + ";" + docsGenderCode + ";" + docsDob + ";" + docsNationalityCode + ";" + flight.DestinationIata + ";" + docsInboundFlightIata + ";" + docsOnwardFlightIata + ";" + docsTypeCode + ";" + docsDocNumber + ";" + docsIssuerNationalityCode + ";" + docoDocType + ";" + docoDocNumber + ";" + docoDocForNatioanalityCode);
                }
                tw.Flush();
                ms.Position = 0;
                var file = Encoding.UTF8.GetString(ms.ToArray());
                Logger.Default.Append(LogLevel.Debug, file);
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Get Content Type
        /// </summary>
        /// <returns></returns>
        public string GetContentType() {
            return "text/plain";
        }

        /// <summary>
        /// Get File Name
        /// </summary>
        /// <param name="flight"></param>
        /// <returns></returns>
        public string GetFileName(FlightResponse flight) {
            string flightDate = string.Empty;
            if (flight.AdepFlightDate.HasValue)
                flightDate = flight.AdepFlightDate.Value.ToString("ddMMMyyyy", enUs).ToUpper();
            else
                flightDate = flight.AdesFlightDate.Value.ToString("ddMMMyyyy", enUs).ToUpper();
            return flight.AirlineIataCode + flight.FlightNumber + flightDate + flight.DepartureIata + flight.DestinationIata + "_GermanyApisFile(CSV).txt";
        }
    }
}
    }
}
