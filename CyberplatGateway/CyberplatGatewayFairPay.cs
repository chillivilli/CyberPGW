using Gateways;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Cyberplat
{ 
    public class CyberplatGatewayFairPay : CyberplatGateway, IGateway
    {
        public CyberplatGatewayFairPay() : base()
        {
        }

        public CyberplatGatewayFairPay(CyberplatGatewayFairPay cyberplatGatewayFairPay) : base(cyberplatGatewayFairPay)
        {
        }

        /// <summary>
        /// Возвращает копию объекта
        /// </summary>
        public override IGateway Clone()
        {
            return new CyberplatGatewayFairPay(this);
        }

        public override string ProcessOnlineCheck(NewPaymentData paymentData, object operatorData)
        {
            string responseString = base.ProcessOnlineCheck(paymentData, operatorData);

            if (paymentData.CyberplatOperatorID == 35)
                responseString += CheckOnlineCyberID_35(paymentData);

            return responseString;
        }

        string CheckOnlineCyberID_35(NewPaymentData paymentData)
        {
            const string msgADDINFO = "ADDINFO=";
            string result = "Нет данных";
            try
            {
                Match match = Regex.Match(paymentData.Params, "NUMBER=(?<ID>\\d+)");
                if (match.Success)
                {
                    string code = match.Groups["ID"].Value;
                    match = Regex.Match(paymentData.Params, "ACCOUNT=(?<MONTH>\\d{2})(?<YEAR>\\d{2})::(?<INSUR>\\d)");
                    if (match.Success)
                    {
                        int year = DateTime.Now.Year - (int.Parse(match.Groups["YEAR"].Value) + 2000);
                        int mon = DateTime.Now.Month - int.Parse(match.Groups["MONTH"].Value) + 1 + year * 12;
                        string insur = match.Groups["INSUR"].Value;

                        string url = "http://www.bm.ru/ru/billing/index.php";
                        WebClient webClient = new WebClient();
                        string data = String.Format("mode=doit&req_code={0}&date_option={1}", code, mon.ToString());
                        webClient.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
                        webClient.Headers.Add(HttpRequestHeader.Referer, url);
                        string response = webClient.UploadString(url, data);
                        string monthReg = "<tr class=\"last\" >\n\t<td><div.*>(?<MONTH>.+) года.*</div></td>";
                        string amountReg = "\n\t<td><div.*>(?<AMOUNT>.+) руб.*</div></td>";
                        string insuranceReg = "\n\t<td><div.*>.*руб.*</div></td>";
                        string amountAllReg = "\n\t<td><div.*>(?<AMOUNTALL>.+) руб.*</div></td>";
                        match = Regex.Match(response, monthReg + amountReg + insuranceReg + amountAllReg, RegexOptions.Multiline);
                        if (match.Success)
                        {
                            if (insur == "1")
                                result = match.Groups["AMOUNTALL"].Value;
                            else
                                result = match.Groups["AMOUNT"].Value;
                            log("CheckOnlineCyberID_35 data:" + data + ", Amount=" + result);
                        }
                        else
                            log("CheckOnlineCyberID_35 data:" + data + ", wrong RESPONSE format");
                    }
                    else
                        log("CheckOnlineCyberID_35 wrong ACCOUNT format");
                }
                else
                    log("CheckOnlineCyberID_35 wrong NUMBER format");
            }
            catch (Exception ex)
            {
                log("CheckOnlineCyberID_35 exception:" + ex.Message);
            }
            return msgADDINFO + result + "\r\n";
        }
    }
}
