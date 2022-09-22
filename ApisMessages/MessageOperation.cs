using AviatorManager.ViewModel.Aodb;
using Bridge.App.Core.Parameters;
using Bridge.Common;
using Bridge.Common.Logging;
using DCS.App.Service.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DCS.App.Service.Service.Exporter
{
    public static class MessageOperation
    {
        private static CultureInfo enUs = new CultureInfo("en-Us");
        public static List<string> SinglePartOrMultiPart(string operationType, string partType, List<string> messageListHeader, List<string> messageListBody, List<string> messageListFoother)
        {
            var channelCharecterLimit = GeneralParameters.GetParameterTemplateValues("DCS_FLIGHT_EXPORTER_OPTIONS", operationType);
            int channelCharecterLimitValue = string.IsNullOrEmpty(channelCharecterLimit.ToString()) ? Convert.ToInt32(channelCharecterLimit.FirstOrDefault().Value) : 2000;

            List<string> messageList = new List<string>();
            List<string> messageDividedPassengerList = new List<string>();
            int messageListBodyCharecterCount = 0;
            int messageListHeaderCharecterCount = 0;
            int messageListFootherCharecterCount = 0;
            int partIdentifier = 1;

            foreach (var messageHeader in messageListHeader)
            {
                messageListHeaderCharecterCount = messageListHeaderCharecterCount + messageHeader.Length;
            }
            foreach (var messageFoother in messageListFoother)
            {
                messageListFootherCharecterCount = messageListFootherCharecterCount + messageFoother.Length;
            }

            if (partType == "S")
            {
                messageList.AddRange(messageListHeader);
                messageList.AddRange(messageListBody);
                messageList.AddRange(messageListFoother);
                messageList.Add(partIdentifier.ToString().PadLeft(3, '0') + "PartIdentifier:");//PartIdentifier Mesaj liste eklyoruz.
                return messageList;
            }
            else if (partType == "M")
            {
                //Multi-part seçeneği için partlar en fazla channelCharecterLimitValue karakter olacak.
                for (var i = 0; i < messageListBody.Count; i++)
                {
                    messageListBodyCharecterCount = messageListBodyCharecterCount + messageListBody[i].Length + 1; // +1:Line 
                    if (messageListBodyCharecterCount <= (channelCharecterLimitValue - messageListHeaderCharecterCount - messageListFootherCharecterCount))
                    {
                        messageDividedPassengerList.Add(messageListBody[i]);
                    }
                    else
                    {
                        for (var j = messageDividedPassengerList.Count; j >= 0; j--)
                        {
                            if (messageDividedPassengerList[j - 1].Substring(0, 1) == ("1"))
                            { // 1:Passenger Name ve Surname bilgilerinin yazıldığı segment. Karakter sayısında aşım olduğu zaman yolcu bilgileri farklı partlarda olmasın diye bu kontrol eklendi.
                                i = j - 1;
                                j = 0;
                            }
                        }
                        //messageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                        messageList.AddRange(messageListHeader);
                        for (var a = 0; a < i; a++)
                        {
                            messageList.Add(messageDividedPassengerList[a]);
                            messageListBody.Remove(messageDividedPassengerList[a]);
                        }
                        messageList.AddRange(messageListFoother);
                        //partları ayırmak için eklendi.
                        messageList.Add("*******************************************************************");
                        messageDividedPassengerList = new List<string>();
                        messageListBodyCharecterCount = 0;
                        i = -1;
                        partIdentifier++;
                    }
                }
                if (messageDividedPassengerList.Count > 0 && messageListBodyCharecterCount < (channelCharecterLimitValue - messageListHeaderCharecterCount - messageListFootherCharecterCount))
                {
                    messageList.AddRange(messageListHeader);
                    messageList.AddRange(messageDividedPassengerList);
                    messageList.AddRange(messageListFoother);
                }
                messageList.Add(partIdentifier.ToString().PadLeft(3, '0') + "PartIdentifier:");//PartIdentifier Mesaj liste eklyoruz.
                return messageList;
            }
            else
            {
                Logger.Default.Append(LogLevel.Error, string.Format(ErrorResource.PartIdentifierUnknown, partType));
                return null;
            }
        }
        public static List<string> SinglePartOrMultiPart(string operationType, string partType, List<string> messageListHeader, List<string> messageListHeaderDestination, List<string> messageListBody, List<string> messageListFoother)
        {
            var channelCharecterLimit = GeneralParameters.GetParameterTemplateValues("DCS_FLIGHT_EXPORTER_OPTIONS", operationType);
            int channelCharecterLimitValue = string.IsNullOrEmpty(channelCharecterLimit.ToString()) ? Convert.ToInt32(channelCharecterLimit.FirstOrDefault().Value) : 2000;

            List<string> messageList = new List<string>();
            List<string> messageDividedPassengerList = new List<string>();
            int messageListBodyCharecterCount = 0;
            int messageListHeaderCharecterCount = 0;
            int messageListFootherCharecterCount = 0;
            int partIdentifier = 1, passengerCount = 0;


            foreach (var messageHeader in messageListHeader)
            {
                messageListHeaderCharecterCount = messageListHeaderCharecterCount + messageHeader.Length;
            }
            foreach (var messageFoother in messageListFoother)
            {
                messageListFootherCharecterCount = messageListFootherCharecterCount + messageFoother.Length;
            }

            if (partType == "S")
            {
                messageList.AddRange(messageListHeader);
                messageList.AddRange(messageListBody);
                messageList.AddRange(messageListFoother);
                messageList.Add(partIdentifier.ToString().PadLeft(3, '0') + "PartIdentifier:");//PartIdentifier Mesaj liste eklyoruz.
                return messageList;
            }
            else if (partType == "M")
            {
                //Multi-part seçeneği için partlar en fazla channelCharecterLimitValue karakter olacak.
                for (var i = 0; i < messageListBody.Count; i++)
                {
                    messageListBodyCharecterCount = messageListBodyCharecterCount + messageListBody[i].Length + 1; // +1:Line 
                    if (messageListBodyCharecterCount <= (channelCharecterLimitValue - messageListHeaderCharecterCount - messageListFootherCharecterCount))
                    {
                        messageDividedPassengerList.Add(messageListBody[i]);
                        if (messageListBody[i].Substring(0, 1) == "1")
                            passengerCount++;
                    }
                    else
                    {
                        for (var j = messageDividedPassengerList.Count; j >= 0; j--)
                        {
                            if (messageDividedPassengerList[j - 1].Substring(0, 1) == ("1"))
                            { // 1:Passenger Name ve Surname bilgilerinin yazıldığı segment. Karakter sayısında aşım olduğu zaman yolcu bilgileri farklı partlarda olmasın diye bu kontrol eklendi.
                                i = j - 1;
                                j = 0;
                                passengerCount--;
                            }
                        }
                        //messageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                        messageList.AddRange(messageListHeader);
                        //PRL ve ETL mesajında DestinationAddres olduğu için
                        if (partIdentifier > 1)
                        {
                            var first = messageListHeaderDestination.Count >= 1 ? Convert.ToInt32(messageListHeaderDestination[0].ToString().Substring(4, messageListHeaderDestination[0].Length - 5)) : 0;
                            var confort = messageListHeaderDestination.Count >= 2 ? Convert.ToInt32(messageListHeaderDestination[1].ToString().Substring(4, messageListHeaderDestination[1].Length - 5)) : 0;
                            var business = messageListHeaderDestination.Count >= 3 ? Convert.ToInt32(messageListHeaderDestination[2].ToString().Substring(4, messageListHeaderDestination[2].Length - 5)) : 0;
                            var economy = messageListHeaderDestination.Count >= 4 ? Convert.ToInt32(messageListHeaderDestination[3].ToString().Substring(4, messageListHeaderDestination[3].Length - 5)) : 0;
                            if (passengerCount <= first)
                                messageList.Add(messageListHeaderDestination[0]);
                            else if (passengerCount <= first + confort)
                                messageList.Add(messageListHeaderDestination[1]);
                            else if (passengerCount <= first + confort + business)
                                messageList.Add(messageListHeaderDestination[2]);
                            else if (passengerCount <= first + confort + business + economy)
                                messageList.Add(messageListHeaderDestination[3]);
                        }
                        for (var a = 0; a < i; a++)
                        {
                            messageList.Add(messageDividedPassengerList[a]);
                            messageListBody.Remove(messageDividedPassengerList[a]);
                        }
                        messageList.AddRange(messageListFoother);
                        //partları ayırmak için eklendi.
                        messageList.Add("*******************************************************************");
                        messageDividedPassengerList = new List<string>();
                        messageListBodyCharecterCount = 0;
                        i = -1;
                        partIdentifier++;
                    }
                }
                if (messageDividedPassengerList.Count > 0 && messageListBodyCharecterCount < (channelCharecterLimitValue - messageListHeaderCharecterCount - messageListFootherCharecterCount))
                {
                    messageList.AddRange(messageListHeader);
                    if (partIdentifier > 1)
                    {
                        var first = messageListHeaderDestination.Count >= 1 ? Convert.ToInt32(messageListHeaderDestination[0].ToString().Substring(4, messageListHeaderDestination[0].Length - 5)) : 0;
                        var confort = messageListHeaderDestination.Count >= 2 ? Convert.ToInt32(messageListHeaderDestination[1].ToString().Substring(4, messageListHeaderDestination[1].Length - 5)) : 0;
                        var business = messageListHeaderDestination.Count >= 3 ? Convert.ToInt32(messageListHeaderDestination[2].ToString().Substring(4, messageListHeaderDestination[2].Length - 5)) : 0;
                        var economy = messageListHeaderDestination.Count >= 4 ? Convert.ToInt32(messageListHeaderDestination[3].ToString().Substring(4, messageListHeaderDestination[3].Length - 5)) : 0;
                        if (passengerCount <= first)
                            messageList.Add(messageListHeaderDestination[0]);
                        else if (passengerCount <= first + confort)
                            messageList.Add(messageListHeaderDestination[1]);
                        else if (passengerCount <= first + confort + business)
                            messageList.Add(messageListHeaderDestination[2]);
                        else if (passengerCount <= first + confort + business + economy)
                            messageList.Add(messageListHeaderDestination[3]);
                    }
                    messageList.AddRange(messageDividedPassengerList);
                    messageList.AddRange(messageListFoother);
                }
                messageList.Add(partIdentifier.ToString().PadLeft(3, '0') + "PartIdentifier:");//PartIdentifier Mesaj liste eklyoruz.
                return messageList;
            }
            else
            {
                Logger.Default.Append(LogLevel.Error, string.Format(ErrorResource.PartIdentifierUnknown, partType));
                return null;
            }
        }
        public static List<string> SinglePartOrMultiPartNameElement(string operationType, string partType, List<string> messageListHeader, List<string> messageListBody, List<string> messageListFoother)
        {
            var channelCharecterLimit = GeneralParameters.GetParameterTemplateValues("DCS_FLIGHT_EXPORTER_OPTIONS", operationType);
            int channelCharecterLimitValue = string.IsNullOrEmpty(channelCharecterLimit.ToString()) ? Convert.ToInt32(channelCharecterLimit.FirstOrDefault().Value) : 2000;

            List<string> messageList = new List<string>();
            List<string> messageDividedPassengerList = new List<string>();
            int messageListBodyCharecterCount = 0;
            int messageListHeaderCharecterCount = 0;
            int messageListFootherCharecterCount = 0;
            int partIdentifier = 1;

            foreach (var messageHeader in messageListHeader)
            {
                messageListHeaderCharecterCount = messageListHeaderCharecterCount + messageHeader.Length;
            }
            foreach (var messageFoother in messageListFoother)
            {
                messageListFootherCharecterCount = messageListFootherCharecterCount + messageFoother.Length;
            }

            if (partType == "S")
            {
                messageList.AddRange(messageListHeader);
                messageList.AddRange(messageListBody);
                messageList.AddRange(messageListFoother);
                messageList.Add(partIdentifier.ToString().PadLeft(3, '0') + "PartIdentifier:");//PartIdentifier Mesaj liste eklyoruz.
                return messageList;
            }
            else if (partType == "M")
            {
                //Multi-part seçeneği için partlar en fazla channelCharecterLimitValue karakter olacak.
                for (var i = 0; i < messageListBody.Count; i++)
                {
                    messageListBodyCharecterCount = messageListBodyCharecterCount + messageListBody[i].Length + 1; // +1:Line 
                    if (messageListBodyCharecterCount <= (channelCharecterLimitValue - messageListHeaderCharecterCount - messageListFootherCharecterCount))
                    {
                        messageDividedPassengerList.Add(messageListBody[i]);
                    }
                    else
                    {
                        //messageList listesine Header-PassengerList-Foother bilgileri eklenip yazdırılıyor.
                        messageList.AddRange(messageListHeader);
                        for (var a = 0; a < i; a++)
                        {
                            messageList.Add(messageDividedPassengerList[a]);
                            messageListBody.Remove(messageDividedPassengerList[a]);
                        }
                        messageList.AddRange(messageListFoother);
                        //partları ayırmak için eklendi.
                        messageList.Add("*******************************************************************");
                        messageDividedPassengerList = new List<string>();
                        messageListBodyCharecterCount = 0;
                        i = -1;
                        partIdentifier++;
                    }
                }
                if (messageDividedPassengerList.Count > 0 && messageListBodyCharecterCount < (channelCharecterLimitValue - messageListHeaderCharecterCount - messageListFootherCharecterCount))
                {
                    messageList.AddRange(messageListHeader);
                    messageList.AddRange(messageDividedPassengerList);
                    messageList.AddRange(messageListFoother);
                }
                messageList.Add(partIdentifier.ToString().PadLeft(3, '0') + "PartIdentifier:");//PartIdentifier Mesaj liste eklyoruz.
                return messageList;
            }
            else
            {
                Logger.Default.Append(LogLevel.Error, string.Format(ErrorResource.PartIdentifierUnknown, partType));
                return null;
            }
        }
        public static List<string> MessageRowCharecterControl(string messageRow, string nextLineStart, List<string> messageListBody)
        {
            for (int line = 1; messageRow.Length > 0; line++)
            {
                if (line > 1)
                    messageRow = nextLineStart + messageRow;

                if (messageRow.Length < 64)
                {
                    messageListBody.Add(messageRow);
                    messageRow = "";
                }
                else
                {
                    messageListBody.Add(messageRow.Substring(0, 63));
                    messageRow = messageRow.Substring(63, messageRow.Length - 63);
                }
            }
            return messageListBody;
        }
        public static List<string> NameElementCharecterControl(string messageRow, List<string> messageListBody)
        {
            for (int line = 1; messageRow.Length > 0; line++)
            {
                if (messageRow.Length < 64)
                {
                    messageListBody.Add(messageRow);
                    messageRow = "";
                }
                else
                {
                    var messageDivideRow = messageRow.Substring(0, 63);
                    int sayı = 1;
                    for (int a = messageDivideRow.Length - 1; a > 0; a--)
                    {
                        if (messageDivideRow[a] != '/')
                        {
                            sayı++;
                        }
                        else if (messageDivideRow[a] == '/')
                            break;
                    }
                    if (sayı == 63)
                    {
                        messageListBody.Add(messageRow.Substring(0, 63));
                        messageRow = messageRow.Substring(63, messageRow.Length - (63));
                    }
                    else
                    {
                        messageListBody.Add(messageRow.Substring(0, 63 - sayı));
                        messageRow = messageRow.Substring(63 - sayı, messageRow.Length - (63 - sayı));
                    }
                }
            }
            return messageListBody;
        }

        public static byte[] GetFileData(List<string> messageList)
        {
            using (var ms = new MemoryStream())
            {
                TextWriter tw = new StreamWriter(ms);
                for (var i = 1; i <= messageList.Count; i++)
                {
                    if (i < messageList.Count)
                        tw.WriteLine(messageList[i - 1]);
                    else if (i == messageList.Count)
                        tw.Write(messageList[i - 1]);
                }
                tw.Flush();
                ms.Position = 0;
                var file = Encoding.UTF8.GetString(ms.ToArray());
                Logger.Default.Append(LogLevel.Debug, file);
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        public static string GetFileName(FlightResponse flight, int partIdentifier, string exporterType)
        {
            exporterType = exporterType.ToUpper();
            var messageType = string.Empty;
            var messageExtension = string.Empty;
            var part = "_PART";
            if (exporterType == ParameterHelper.ExportArrDataDepData.CDMArrData)
            {
                messageType = "_CDM_ARRDATA";
                messageExtension = ".xml";
            }
            else if (exporterType == ParameterHelper.ExportArrDataDepData.CDMDepData)
            {
                messageType = "_CDM_DEPDATA";
                messageExtension = ".xml";
            }
            else if (exporterType == ParameterHelper.ExportArrDataDepData.DMANArrData)
            {
                messageType = "_DMAN_ARRDATA";
                messageExtension = ".xml";
            }
            else if (exporterType == ParameterHelper.ExportArrDataDepData.DMANDepData)
            {
                messageType = "_DMAN_DEPDATA";
                messageExtension = ".xml";
            }
            else if (exporterType == ParameterHelper.AIDXMessagesCode.arrivalAIDX)
            {
                messageType = "_GH_AIDXArrival";
                messageExtension = ".xml";
            }
            else if (exporterType == ParameterHelper.AIDXMessagesCode.departureAIDX)
            {
                messageType = "_GH_AIDXDeparture";
                messageExtension = ".xml";
            }
            else if (exporterType == ParameterHelper.DPIMessagesCode.pDPI)
            {
                messageType = "_P-DPI";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.DPIMessagesCode.eDPI)
            {
                messageType = "_E-DPI";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.DPIMessagesCode.tDPIt)
            {
                messageType = "_T-DPI-t";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.DPIMessagesCode.tDPIs)
            {
                messageType = "_T-DPI-s";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.DPIMessagesCode.aDPI)
            {
                messageType = "_A-DPI-t";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.DPIMessagesCode.cDPI)
            {
                messageType = "_C-DPI-t";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.SummaryList)
            {
                messageType = "_SumarryList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.NorecList)
            {
                messageType = "_NorecList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.PassengerList)
            {
                messageType = "_PassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.MealList)
            {
                messageType = "_MealList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.MalePassengerList)
            {
                messageType = "_MalePassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.InfantPassengerList)
            {
                messageType = "_InfantPassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.FemalePassengerList)
            {
                messageType = "_FemalePassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.CheckedInPassengerList)
            {
                messageType = "_CheckedInPassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.BoardedPassengerList)
            {
                messageType = "_BoardedPassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.FlownPassengerList)
            {
                messageType = "_FlownPassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.OpenPassengerList)
            {
                messageType = "_OpenPassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.NoShowPassengerList)
            {
                messageType = "_NoShowPassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.NotFlownPassengerList)
            {
                messageType = "_NotFlownPassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.OffloadedPassengerList)
            {
                messageType = "_OffloadedPassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.OperationalListCode.FullPassengerList)
            {
                messageType = "_FullPassengerList";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.BelgiumApis)
            {
                messageType = "_BelgiumApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.FinlandApis)
            {
                messageType = "_FinlandApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.JordanEdifactApis)
            {
                messageType = "_JordanApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.IraqApis)
            {
                messageType = "_IraqApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.NetherlandsPaxLst)
            {
                messageType = "_NetherlandsApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPostDepartureForRussiaArrivalPaxLst)
            {
                messageType = "_RussiaApisPostDepartureForRussiaArrival";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPreDepartureForRussiaArrivalPaxLst)
            {
                messageType = "_RussiaApisPreDepartureForRussiaArrival";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPostDepartureForRussiaOverflyPaxLst)
            {
                messageType = "_RussiaApisPostDepartureForRussiaOverfly";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.RussiaApisPreDepartureForRussiaOverflyPaxLst)
            {
                messageType = "_RussiaApisPreDepartureForRussiaOverfly";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.SwissEdifactApis)
            {
                messageType = "_SwissApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.FranceApis)
            {
                messageType = "_FranceApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.LebenonApis)
            {
                messageType = "_LebenonApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.RomaniaApis)
            {
                messageType = "_RomaniaApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.ItalyApis)
            {
                messageType = "_ItalyApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.QatarApis)
            {
                messageType = "_QatarApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.UkraineApis)
            {
                messageType = "_UkraineApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.DenmarkApis)
            {
                messageType = "_DenmarkApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.GermanyCsvPaxLst)
            {
                messageType = "_GermanyApisFile(CSV)";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.GermanyEdifactPaxLst)
            {
                messageType = "_GermanyApisFile(EDIFACT)";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.TurkeyPaxLst)
            {
                messageType = "__TurkeyApisFile";
                messageExtension = ".txt";
                part = null;
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.BmmAdes)
            {
                messageType = "_BmmADES";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.BmmAdep)
            {
                messageType = "_BmmADEP";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.Etl)
            {
                messageType = "_ETL";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.Manifest)
            {
                messageType = "_ICAOPassengerManifest";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.MvtArrival)
            {
                messageType = "_MVTArrival";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.MvtDeparture)
            {
                messageType = "_MVTDeparture";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.Pal)
            {
                messageType = "_PAL";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.Pil)
            {
                messageType = "_PIL";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.Ptm)
            {
                messageType = "_PTM";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.Prl)
            {
                messageType = "_PRL";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.Pfs)
            {
                messageType = "_PFS";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.Psm)
            {
                messageType = "_PSM";
                messageExtension = ".txt";
            }
            else if (exporterType == ParameterHelper.ExporterFactoryCode.Tpm)
            {
                messageType = "_TPM";
                messageExtension = ".txt";
            }

            return flight.AirlineIataCode + flight.FlightNumber + (flight.AdepFlightDate.HasValue ? flight.AdepFlightDate.Value.ToString("ddMMMyyyy", enUs).ToUpper() : flight.AdesFlightDate.Value.ToString("ddMMMyyyy", enUs).ToUpper()) + flight.DepartureIata + flight.DestinationIata + messageType + (partIdentifier > 0 ? (!string.IsNullOrEmpty(part) ? part : null) + partIdentifier.ToString().PadLeft(2, '0') : null) + messageExtension;
        }

        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}