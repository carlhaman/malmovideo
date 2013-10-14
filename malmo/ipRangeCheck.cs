using System.Net;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Web;


namespace malmo
{
    public struct IpRange
    {
        public IPAddress LowerIP;
        public IPAddress UpperIP;

        public IpRange(IPAddress lowerIP, IPAddress upperIP)
        {
            LowerIP = lowerIP;
            UpperIP = upperIP;
        }
    }

    public class ipRangeCheck
    {
        public string GetIP4Address()
        {
            string IP4Address = String.Empty;

            foreach (IPAddress IPA in Dns.GetHostAddresses(HttpContext.Current.Request.UserHostAddress))
            {
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }

            if (IP4Address != String.Empty)
            {
                return IP4Address;
            }

            foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    IP4Address = IPA.ToString();
                    break;
                }
            }

            return IP4Address;
        }

        public bool CheckIsIpInRange(string adress, List<IpRange> rangeList)
        {
            foreach (var range in rangeList)
            {
                List<int> adressInt = adress.Split('.').Select(str => int.Parse(str)).ToList();
                List<int> lowerInt = range.LowerIP.ToString().Split('.').Select(str => int.Parse(str)).ToList();
                List<int> upperInt = range.UpperIP.ToString().Split('.').Select(str => int.Parse(str)).ToList();

                if (adressInt[0] >= lowerInt[0] && adressInt[0] < upperInt[0])
                {
                    return true;
                }
                else if (adressInt[0] >= lowerInt[0] && adressInt[0] == upperInt[0])
                {
                    if (adressInt[1] >= lowerInt[1] && adressInt[1] < upperInt[1])
                    {
                        return true;
                    }
                    else if (adressInt[1] >= lowerInt[1] && adressInt[1] == upperInt[1])
                    {
                        if (adressInt[2] >= lowerInt[2] && adressInt[2] < upperInt[2])
                        {
                            return true;
                        }
                        else if (adressInt[2] >= lowerInt[2] && adressInt[2] == upperInt[2])
                        {
                            if (adressInt[3] >= lowerInt[3] && adressInt[3] <= upperInt[3])
                            {
                                return true;
                            }
                        }

                    }

                }
            }
            return false;
        }

    }
}