using HTCPCP_Server.Enumerations;
using HTCPCP_Server.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace HTCPCP_Server.Helpers
{
    internal static class RequestParser
    {
        /// <summary>
        /// Parses a request line
        /// </summary>
        /// <param name="requestline">The request line to parse</param>
        /// <returns>Returns the request in a parsed form</returns>
        public static Request ParseRequest(string requestline)
        {
            bool version = false;
            string resource = "";
            var type = ParseRequestLine(requestline, out version, out resource);

            if (!version)
            {
                return new Request(type, StatusCode.BadRequest);
            }

            switch (type)
            {
                case HTCPCPType.Post:
                    return new Request(type, StatusCode.Deprecated);
                case HTCPCPType.When:
                    return new Request(type, StatusCode.NotImplemented);
                case HTCPCPType.Propfind:
                    return new Request(type, StatusCode.NotImplemented);
                case HTCPCPType.Get:
                    return new Request(type, StatusCode.NotImplemented);
                case HTCPCPType.Unknown:
                    return new Request(type, StatusCode.BadRequest);
                case HTCPCPType.Put:
                    return new Request(type, StatusCode.BadRequest);
                default:
                    break;
            }

            List<Tuple<Option, int>> additions = new List<Tuple<Option, int>>();
            string pot = "";
            if (!ParseResource(resource, additions, out pot))
            {
                return new Request(type, StatusCode.BadRequest);
            }

            return new Request(type, pot, additions);
        }

        /// <summary>
        /// Parses the header fields. Looks for message type and content length
        /// </summary>
        /// <param name="header">The header string</param>
        /// <param name="contentLength">TThe content Length</param>
        /// <returns>Returns true if the message type is message/coffeepot, otherwise false</returns>
        public static bool ParseHeaderFields(string header, out int contentLength)
        {
            contentLength = 0;
            bool contentType = false;
            string[] fields = header.Split("\r\n");

            foreach (string field in fields)
            {
                if(field == "Content-Type: message/coffeepot")
                {
                    contentType = true;
                }

                if(field.Contains("Content-Length: ")) 
                { 
                    if(!int.TryParse(field.AsSpan(16), out contentLength))
                    {
                        contentLength = 0;
                    }
                }
            }

            return contentType;
        }

        /// <summary>
        /// Checks whether the body is conform with message/coffeepot
        /// </summary>
        /// <param name="body">The body</param>
        /// <param name="start">This out parameter is set to true if the command is to start the brewing process</param>
        /// <returns>Returns true if it is conform with message/coffeepot</returns>
        public static bool BodyConform(string body, out bool start)
        {
            body = body.Trim('\0');
            if(body == "coffee-message-body = stop")
            {
                start = false;
                return true;
            }else if(body == "coffee-message-body = start")
            {
                start = true;
                return true;
            }

            start = false;
            return false;
        }

        /// <summary>
        /// Parses the request line of the htcpcp request
        /// </summary>
        /// <param name="request">The request line</param>
        /// <param name="version">out parameter, true if the version is correct</param>
        /// <param name="resource">out parameter, contains the resource string if available</param>
        /// <returns></returns>
        public static HTCPCPType ParseRequestLine(string request, out bool version, out string resource)
        {
            request = request.Replace("\r\n", "");
            string[] parts = request.Split(' ');

            if(parts.Length != 3)
            {
                version = false;
                resource = "";
            }
            else
            {
                resource = parts[1];

                version = true;
                Regex versionRegex = new Regex("^HTCPCP\\/1(\\.[0-9]){0,1}$");
                if (!versionRegex.IsMatch(parts[2]))
                {
                    version = false;
                }
            }

            switch (parts[0])
            {
                case "POST":
                    return HTCPCPType.Post;
                case "BREW":
                    return HTCPCPType.Brew;
                case "WHEN":
                    return HTCPCPType.When;
                case "PUT":
                    return HTCPCPType.Put;
                case "PROPFIND":
                    return HTCPCPType.Propfind;
                case "GET":
                    return HTCPCPType.Get;
                default:
                    return HTCPCPType.Unknown;
            }
        }

        /// <summary>
        /// Parses the resource into the pot id and the additions
        /// </summary>
        /// <param name="resource">The resource string to parse</param>
        /// <param name="additions">The additions list</param>
        /// <returns>Returns true if the resource was parsable</returns>
        public static bool ParseResource(string resource, List<Tuple<Option, int>> additions, out string pot)
        {
            resource = HttpUtility.UrlDecode(resource, System.Text.UnicodeEncoding.UTF8);
            pot = "pot-0";

            Regex schemeRegex = new Regex("^(koffie)|(akeita)|(koffee)|(kahva)|(kafe)|(kava)|(kaffe)|(coffee)|(kafo)|(kohv)|(kahvi)|(\u004Baffee)|(qəhvə)|(\u0642\u0647\u0648\u0629)|(\u0063\u0061\u0066\u00e9)|(\u5496\u5561)|(\u006b\u00e1\u0076\u0061)|(\u03ba\u03b1\u03c6\u03ad)|(\u0915\u094c\u092b\u0940)|(\u30b3\u30fc\u30d2\u30fc)|(\ucee4\ud53c)|(\u043a\u043e\u0444\u0435)|(\u0e01\u0e32\u0e41\u0e1f)$");

            string[] identifierSplit = resource.Split(':');
            if(identifierSplit.Length != 2 ) 
            {
                return false;
            }

            if (!schemeRegex.IsMatch(identifierSplit[0]))
            {
                return false;
            }

            if (identifierSplit[1].Length == 0)
            {
                return true;
            }

            if (identifierSplit[1].StartsWith("//"))
            {
                identifierSplit[1] = identifierSplit[1].Substring(2);
                var index = identifierSplit[1].IndexOfAny(new[] {'/', '?'});

                if (index != -1)
                {
                    identifierSplit[1] = identifierSplit[1].Remove(0, index);
                }
            }
            
            if (identifierSplit[1].StartsWith("/"))
            {
                var index = identifierSplit[1].IndexOf('?');

                if(index != -1)
                {
                    pot = identifierSplit[1].Substring(1, index - 1);
                    identifierSplit[1] = identifierSplit[1].Remove(0, index);
                }
                else
                {
                    pot = identifierSplit[1].Substring(1);
                }
            }
            
            if (identifierSplit[1].StartsWith("?"))
            {
                string[] additionsArray = identifierSplit[1].Split('?');
                foreach(string addition in additionsArray)
                {
                    var opt = ParseAddition(addition);

                    var tuple = additions.Where((tp) => tp.Item1 == opt).FirstOrDefault();
                    if(tuple == null)
                    {
                        tuple = new Tuple<Option, int>(opt, 1);
                    }
                    else
                    {   
                        additions.Remove(tuple);
                        tuple = new Tuple<Option, int>(tuple.Item1, tuple.Item2 + 1);
                    }

                    additions.Add(tuple);
                }
            }

            return true;
        }

        /// <summary>
        /// Translates an addition into the correct enum. Defaults to Whole Milk if addition unknown
        /// </summary>
        /// <param name="addition">The addition to parse</param>
        /// <returns>Returns the corresponding enum</returns>
        public static Option ParseAddition(string addition)
        {
            switch (addition)
            {
                case "Cream":
                    return Option.MilkCream;
                case "Half-and-half":
                    return Option.MilkHalfAndHalf;
                case "Whole-milk":
                    return Option.MilkWholeMilk;
                case "Part-Skim":
                    return Option.MilkPartSkim;
                case "Skim":
                    return Option.MilkSkim;
                case "Non-Dairy":
                    return Option.MilkNonDairy;
                case "Vanilla":
                    return Option.SyrupVanilla;
                case "Almond":
                    return Option.SyrupAlmond;
                case "Raspberry":
                    return Option.SyrupRaspberry;
                case "Chocolate":
                    return Option.SyrupChocolate;
                case "Whisky":
                    return Option.AlcoholWhisky;
                case "Rum":
                    return Option.AlcoholRum;
                case "Kahlua":
                    return Option.AlcoholKahlua;
                case "Aquavit":
                    return Option.AlcoholAquavit;
                default:
                    return Option.MilkWholeMilk;
            }
        }
    }
}
