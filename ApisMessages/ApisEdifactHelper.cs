using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApisMessages
{
    public class ApisEdifactHelper
    {
        
        public const string enUs = new CultureInfo("en-US");

        public const string yearToMin = DateTime.Now.ToString("yyMMdd", enUs) + ":" + DateTime.Now.ToString("HHmm", enUs);
        public const string yearToDay = DateTime.Now.ToString("yyMMdd", enUs);

        Random r = new Random();
        public const string BZcode = r.Next(100000000, 999999999).ToString();
        public const string GEcode = r.Next(100000000, 999999999).ToString();
        public const string HTcode = r.Next(100000000, 999999999).ToString();
        public const string unique = r.Next(100000000, 999999999).ToString();

        public const string version02B = "02B";
        public const string version05B = "05B";
        public const string version12B = "12B";
        public const string version15B = "15B";

        public const string una = "UNA:+.?";
        public const string unaEnd = "*'";
        public const string unb = "UNB+UNOA:4+";
        public const string sender = "BRIDGEIS+";

        public const string unbEnd = "++APIS";
        public const string ung = "UNG+PAXLST+";
        public const string typeVersionNum= "+UN+D:";
        public const string unh = "UNH+";
        public const string unhMid1 = " +PAXLST:D:";
        public const string unhMid2 = ":UN:IATA";
        public const string unhSinglePart = "+01:F'";
        public const string bgm745 = "BGM+745";//Indicates passenger list
        public const string bgm250 = "BGM+250";//Indicates crew list declaration
        //public const string bgm266 = "BGM+266";//Indicates change in flight status@ JUST FOR INTERACTIVE APIS MESSAGES
        public const string bgm336 = "BGM+366";//Indicates master crew list declaration
        public const string bgm655 = "BGM+655";//Indicates Gate Pass

        public const string changePassengerData = "CP'";
        public const string cancelReservation = "XR'";
        public const string reductionOnParty = "RP'";
        public const string crewFlightClose = "CL'";
        public const string idCrewNotOnBoard = "CLNB'";
        public const string idCrewOnBoard  = "CLOB'";

        
        public const string rffTn = "RFF+TN:";
        public const string nadMs = "NAD+MS+++DEGIRMENCI:GAMZE'";
        public const string com= "COM+212 211 1003:TE+212 211 1004:FX+SUPPORT BRIDGEIST COM:EM'";
        public const string tdt20 = "TDT+20+";//For arriving or departing flight
        public const string tdt34 = "TDT+34+";// Over-flight.
        public const string loc125 = "LOC+125+";//Indicates the last airport of departure from a foreign country
        public const string loc87 = "LOC+87+";//Indicates the first airport of arrival in the country of destination
        public const string loc92 = "LOC+92+";//Indicates the next airport in the country of destination, For a multi-sector progressive flight
        public const string loc130 = "LOC+130+";//Indicates the final destination airport in the country of destination, For a multi-sector progressive flight
        public const string dtm189 = "DTM+189+";//Indicates the scheduled departure date and time of the flight
        public const string dtmEnd = ":201'";
        public const string dtm232 = "DTM+232+";//Indicates the scheduled arrival date of flight 
        public const string nadFl = "NAD+FL+++";
        public const string nadDdt = "NAD+DDT+++";//Indicates an ‘In Transit’ Crew member.                                               
        public const string nadFm = "NAD+FM+++";//Indicates a Crew Member
        public const string nadDdu = "NAD+DDU+++";//Indicates an ‘In Transit’ Passenger.                                                 
                                  //gender info
        public const string attF = "ATT+2++F'";
        public const string attM = "ATT+2++M'";
        public const string attX = "ATT+2++X'";
        public const string attU = "ATT+2++U'";

        public const string dtm329 = "DTM+329:";
        public const string meaCt = "MEA+CT++:";//number of bags
        public const string meaWtKg = "MEA+WT++KGM:";//weight(kg)
        public const string meaWtLbr = "MEA+WT++LBR:";//weight(pounds)
        public const string gei173 = "GEI+4+173'";//Indicates that the information contained for this passenger has been verified. 
        public const string gei174 = "GEI+4+174'";//for information not verified
        public const string ftx = "FTX+BAG+++";
        public const string loc22 = "LOC+22+";//For intransit passengers or crew members or for progressive clearance flights, indicates the airport where a passenger or crew member will complete clearance procedures, 
        public const string loc174 = "LOC+174+";//Indicates the country of residence as per ICAO Document 9303 ISO 3166 (3 alpha).
        public const string loc178 = "LOC+178+";//Indicates the airport where a passenger or crew member began their
        public const string loc179 = "LOC+179+";//For intransit passengers or crew members or for progressive clearance flights, indicates the airport where a passenger or crew member will end their journey

        public const string loc180 = "LOC+180+:::";//Indicates the place of birth as per ICAO Document 9303.
        public const string emp1 = "EMP+1+CR1:";//for cockpit crew or individuals inside cockpit
        public const string emp2 = "EMP+1+CR2:"; //for cabin crew
        public const string emp3 = "EMP+1+CR3:";//for airline operation management with cockpit access
        public const string emp4 = "EMP+1+CR4:";//for cargo non cockpit crew and/or non-crew individuals
        public const string emp5 = "EMP+1+CR5:";//pilots on board but not on duty
        public const string nat2 = "NAT+2+";
        public const string rffAvf = "RFF+AVF:"; //Indicates passenger reservation reference number

        //NEDEN BR YAZILMIŞ?
        public const string rffAbo = "RFF+ABO:BR";//Indicates Unique Passenger Reference
        public const string rffSea = "RFF+SEA:";//Indicates assigned Seat identification
        public const string rffAea = "RFF+AEA:";//Government agency22 reference number (Optionally issued by a state to facilitate booking and travel).
        public const string rffCr = "RFF+CR:";//Customer Reference Number. Frequent flyer or frequent traveler reference
        public const string docP = "DOC+P+";
        public const string docV = "DOC+V+";
        public const string docI = "DOC+I+";
        public const string docC = "DOC+C+";
        public const string docIP = "DOC+IP+";
        public const string docAC = "DOC+AC+";
        public const string docA = "DOC+A+";
        public const string docF = "DOC+F+";
        public const string dtm36 = "DTM+36:";
        public const string dtm182 = "DTM+182:";
        public const string loc91 = "LOC+91+";
        public const string cnt42 = "CNT+42:";//number of passengers
        public const string cnt41 = "CNT+41:";//number of crew members
        public const string unt = "UNT+";
        public const string une = "UNE+1+";
        public const string unz = "UNZ+1+";
        public const string apos = "'";
        public const string colon = ":";
        public const string plus = "+";
        public const string threePlus = "+++";
        public const string threeColon = ":::";
    }
}
