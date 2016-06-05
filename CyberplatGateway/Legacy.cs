////#define EMULATION
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Xml;
//using System.Net;
//using System.Net.Cache;
//using System.Web;
//using System.Data;
//using System.IO;
//using System.Text.RegularExpressions;
//using System.Threading;
//using Gateways.Utils;
//using org.CyberPlat;
//using PPS.MyTools;

//namespace Gateways
//{
//    public class LegacyCyberplatGateway : BaseGateway, IGateway, IGateOnlinePayment
//    {
     
//        protected bool detailLogEnabled = true;

//        private string cyberplatProcessingUrl = "";

//        private int cyberplatAP = 0;

//        private int cyberplatOP = 0;

//        private int cyberplatSD = 0;

//        private IPrivKey cyberplatSecretKey = null;

//        private IPrivKey cyberplatSecretStatKey = null;

//        private IPrivKey cyberplatPublicKey = null;

//        private uint serial = 904291u;

//        private string testUrl = "/cgi-bin/es/es_pay_check.cgi";

//        private string testPhone = "9166731169";

//        private string proxyUrl;

//        private string proxyLogin;

//        private string proxyPassword;

//        private bool use_second_point;

//        private int cyberplatAP_2 = 0;

//        private int cyberplatOP_2 = 0;

//        private string password2;

//        private IPrivKey cyberplatSecretKey_2 = null;

//        private string cyberplatStatUrl = "";

//        private static string msgAP = "AP=";

//        private static string msgOP = "OP=";

//        private static string msgSD = "SD=";

//        private static string msgCOMMENT = "COMMENT=";

//        private static string msgSESSION = "SESSION=";

//        private static string msgNUMBER = "NUMBER=";

//        private static string msgACCOUNT = "ACCOUNT=";

//        private static string msgAMOUNT = "AMOUNT=";

//        private static string msgAMOUNT_ALL = "AMOUNT_ALL=";

//        private static string msgERROR = "ERROR=";

//        private static string msgRESULT = "RESULT=";

//        private static string msgREQ_TYPE1 = "REQ_TYPE=1";

//        private static string msgCYBERERROR = "CYBERERROR=";

//        private static string msgCYBERRESULT = "CYBERRESULT=";

//        private static string msgNEXTPAYMENT = "NEXTPAYMENT";

//        private static string msgURL = "URL=";

//        private static string msgFILENAME = "FILENAME=";

//        private static string msgOFFSET = "OFFSET=";

//        private static string msgSIZE = "SIZE=";

//        private static string msgCONTENT = "CONTENT";

//        private static string msgDOWNLOAD = "DOWNLOAD=";

//        private static string msgUPLOAD = "UPLOAD=";

//        private static object tr_counterLock = new object();

//        private static int tr_counter = 0;

//        public LegacyCyberplatGateway()
//        {
//            base.Balance = "";
//        }

//        public LegacyCyberplatGateway(LegacyCyberplatGateway cyberplatGateway)
//        {
//            this.cyberplatProcessingUrl = cyberplatGateway.cyberplatProcessingUrl;
//            this.cyberplatAP = cyberplatGateway.cyberplatAP;
//            this.cyberplatOP = cyberplatGateway.cyberplatOP;
//            this.cyberplatSD = cyberplatGateway.cyberplatSD;
//            this.cyberplatSecretKey = cyberplatGateway.cyberplatSecretKey;
//            this.cyberplatPublicKey = cyberplatGateway.cyberplatPublicKey;
//            this.cyberplatSD = cyberplatGateway.cyberplatSD;
//            this.cyberplatSecretStatKey = cyberplatGateway.cyberplatSecretStatKey;
//            this.cyberplatPublicKey = cyberplatGateway.cyberplatPublicKey;
//            this.cyberplatStatUrl = cyberplatGateway.cyberplatStatUrl;
//            this.paySystemKeyID = cyberplatGateway.paySystemKeyID;
//            this.use_second_point = cyberplatGateway.use_second_point;
//            this.cyberplatAP_2 = cyberplatGateway.cyberplatAP_2;
//            this.cyberplatOP_2 = cyberplatGateway.cyberplatOP_2;
//            this.password2 = cyberplatGateway.password2;
//            this.cyberplatSecretKey_2 = cyberplatGateway.cyberplatSecretKey_2;
//            this.serial = cyberplatGateway.serial;
//            this.testUrl = cyberplatGateway.testUrl;
//            this.testPhone = cyberplatGateway.testPhone;
//            this.proxyUrl = cyberplatGateway.proxyUrl;
//            this.proxyLogin = cyberplatGateway.proxyLogin;
//            this.proxyPassword = cyberplatGateway.proxyPassword;
//            base.GateProfileID = cyberplatGateway.GateProfileID;
//            this.showAmountAll = cyberplatGateway.showAmountAll;
//            base.Copy(cyberplatGateway);
//        }

//        ~LegacyCyberplatGateway()
//        {
//            if (this.cyberplatSecretKey != null)
//            {
//                this.cyberplatSecretKey.closeKey();
//            }
//            if (this.cyberplatSecretStatKey != null)
//            {
//                this.cyberplatSecretStatKey.closeKey();
//            }
//            if (this.cyberplatPublicKey != null)
//            {
//                this.cyberplatPublicKey.closeKey();
//            }
//            if (this.cyberplatSecretStatKey != null)
//            {
//                this.cyberplatSecretStatKey.closeKey();
//            }
//        }

//        public void Initialize(string data)
//        {
//            try
//            {
//                base.log("CyberplatGateway::Initialize, GateProfileID=" + base.GateProfileID.ToString());
//                XmlDocument xmlDocument = new XmlDocument();
//                xmlDocument.LoadXml(data);
//                this.cyberplatProcessingUrl = xmlDocument.DocumentElement["server_url"].InnerText;
//                this.cyberplatAP = int.Parse(xmlDocument.DocumentElement["ap"].InnerText);
//                this.cyberplatAP_2 = int.Parse(xmlDocument.DocumentElement["ap2"].InnerText);
//                this.cyberplatOP = int.Parse(xmlDocument.DocumentElement["op"].InnerText);
//                this.cyberplatOP_2 = int.Parse(xmlDocument.DocumentElement["op2"].InnerText);
//                this.cyberplatSD = int.Parse(xmlDocument.DocumentElement["sd"].InnerText);
//                this.paySystemKeyID = this.cyberplatSD.ToString();
//                this.cyberplatSecretKey = IPriv.openSecretKey(xmlDocument.DocumentElement["secret_key"].InnerText, xmlDocument.DocumentElement["password"].InnerText);
//                this.cyberplatSecretKey_2 = IPriv.openSecretKey(xmlDocument.DocumentElement["secret_key2"].InnerText, xmlDocument.DocumentElement["password2"].InnerText);
//                this.cyberplatPublicKey = IPriv.openPublicKey(xmlDocument.DocumentElement["public_key"].InnerText, uint.Parse(xmlDocument.DocumentElement["serial"].InnerText));
//                if (xmlDocument.DocumentElement["use_second_point"] != null && (xmlDocument.DocumentElement["use_second_point"].InnerText.ToLower() == "true" || xmlDocument.DocumentElement["use_second_point"].InnerText == "1"))
//                {
//                    this.use_second_point = true;
//                }
//                else
//                {
//                    this.use_second_point = false;
//                }
//                try
//                {
//                    this.proxyUrl = xmlDocument.DocumentElement["proxy_url"].InnerText;
//                    this.proxyLogin = xmlDocument.DocumentElement["proxy_login"].InnerText;
//                    this.proxyPassword = xmlDocument.DocumentElement["proxy_password"].InnerText;
//                    base.log("CyberplatGateway::Initialize, proxy parameters OK");
//                }
//                catch
//                {
//                    base.log("CyberplatGateway::Initialize, proxy disabled");
//                }
//                if (this.cyberplatAP == 0 || this.cyberplatProcessingUrl == "")
//                {
//                    throw new Exception("Error! One or more parameters not set");
//                }
//                try
//                {
//                    this.cyberplatStatUrl = xmlDocument.DocumentElement["stat_url"].InnerText;
//                    this.cyberplatSecretStatKey = IPriv.openSecretKey(xmlDocument.DocumentElement["stat_secret_key"].InnerText, xmlDocument.DocumentElement["stat_password"].InnerText);
//                }
//                catch (Exception ex)
//                {
//                    base.log("CyberplatGateway::Initialize, stat params exception:" + ex.ToString());
//                }
//                try
//                {
//                    this.serial = uint.Parse(xmlDocument.DocumentElement["serial"].InnerText);
//                    this.testUrl = xmlDocument.DocumentElement["test_url"].InnerText;
//                    this.testPhone = xmlDocument.DocumentElement["test_phone"].InnerText;
//                }
//                catch
//                {
//                }
//            }
//            catch (Exception ex)
//            {
//                base.log("CyberplatGateway::Initialize exception: " + ex.ToString());
//                throw ex;
//            }
//        }

//        public string CheckSettings()
//        {
//            CyberplatGateway.ErrorStatus errorStatus = CyberplatGateway.ErrorStatus.UnknownError;
//            string text = "No response.";
//            try
//            {
//                string text2 = ((int)(DateTime.Now - DateTime.Now.Date).TotalMilliseconds).ToString();
//                while (text2.Length < 12)
//                {
//                    text2 = "0" + text2;
//                }
//                text2 = text2.Substring(text2.Length - 12, 12);
//                string text3 = string.Format("SD={0}\r\nAP={1}\r\nOP={2}\r\nSESSION={3}\r\nNUMBER={4}\r\nAMOUNT=10.00\r\nAMOUNT_ALL=10.00\r\n{5}", new object[]
//                {
//                    this.cyberplatSD,
//                    this.cyberplatAP,
//                    this.cyberplatOP,
//                    DateTime.Now.ToString("ddMMyyyy") + text2,
//                    this.testPhone,
//                    (this.testPhone != "1111111111") ? "REQ_TYPE=1\r\nCOMMENT=MONITORING_CHECK\r\n" : ""
//                });
//                errorStatus = CyberplatGateway.ErrorStatus.ProcessingSecretKeyError;
//                text3 = this.cyberplatSecretKey.signText(text3);
//                errorStatus = CyberplatGateway.ErrorStatus.UnknownError;
//                text3 = HttpUtility.UrlEncode(text3);
//                text3 = "inputmessage=" + text3;
//                WebClient webClient = new WebClient();
//                webClient.Encoding = Encoding.GetEncoding(1251);
//                errorStatus = CyberplatGateway.ErrorStatus.CyberplatConnectionError;
//                string text4 = webClient.UploadString(this.cyberplatProcessingUrl + this.testUrl, text3);
//                errorStatus = CyberplatGateway.ErrorStatus.ProcessingPublicKeyError;
//                text4 = this.cyberplatPublicKey.verifyText(text4);
//                text = text4;
//                errorStatus = CyberplatGateway.ErrorStatus.InternalProcessingSecretKeyError;
//                string str = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Keys");
//                if (File.Exists(str + "\\internal.keyInfo"))
//                {
//                    XmlDocument xmlDocument = new XmlDocument();
//                    xmlDocument.LoadXml(File.ReadAllText(str + "\\internal.keyInfo", Encoding.GetEncoding(1251)));
//                    IPrivKey privKey = IPriv.openSecretKey(xmlDocument.DocumentElement["secret_key"].InnerText, xmlDocument.DocumentElement["password"].InnerText);
//                    text4 = privKey.signText(text4);
//                    errorStatus = CyberplatGateway.ErrorStatus.OK;
//                    privKey.closeKey();
//                }
//                try
//                {
//                    text += "\r\nПроверка статистики Киберплат, RESULT=";
//                    byte[] array;
//                    if (!this.TransactCyberplatStatistics(this.cyberplatStatUrl, new DateTime(1990, 1, 1), this.cyberplatSD, this.cyberplatSecretStatKey, this.cyberplatPublicKey, out array))
//                    {
//                        throw new Exception("Не удалось получить данные Cyberplat,\r\nпроверьте правильность ключей и связи с сервером Cyberplat.");
//                    }
//                    text += "OK";
//                }
//                catch (Exception ex)
//                {
//                    text += ex.Message;
//                }
//            }
//            catch (IPrivException ex2)
//            {
//                text = ex2.ToString();
//            }
//            catch
//            {
//            }
//            return errorStatus.ToString() + "\r\n" + text;
//        }

//        public void ProcessPayment(object paymentData, object operatorData, object exData)
//        {
//            DataRow dataRow = paymentData as DataRow;
//            string arg = dataRow["InitialSessionNumber"] as string;
//            try
//            {
//                this.ProcessOfflinePayment(paymentData, operatorData, exData);
//            }
//            catch (Exception ex)
//            {
//                base.log(string.Format("ProcessOfflinePayment KeyID={2}(initial_session={0}) exception: {1}", arg, ex.Message, this.keyID));
//            }
//        }

//        public OnlinePaymentResponse ProcessOnlinePayment(DataRow paymentRow, object operatorData)
//        {
//            OnlinePaymentResponse onlinePaymentResponse = new OnlinePaymentResponse();
//            string arg = paymentRow["InitialSessionNumber"] as string;
//            try
//            {
//                onlinePaymentResponse.StatusID = (int)paymentRow["StatusID"];
//                onlinePaymentResponse.ErrorCode = (int)paymentRow["ErrorCode"];
//                if (onlinePaymentResponse.StatusID < 7 || onlinePaymentResponse.StatusID == 103 || onlinePaymentResponse.StatusID == 104)
//                {
//                    onlinePaymentResponse = this.ProcessOfflinePayment(paymentRow, operatorData, null);
//                }
//            }
//            catch (Exception ex)
//            {
//                base.log(string.Format("ProcessOnlinePayment KeyID={2} (initial_session={0}) exception: {1}", arg, ex.Message, this.keyID));
//            }
//            return onlinePaymentResponse;
//        }

//        public OnlinePaymentResponse ProcessOfflinePayment(object paymentData, object operatorData, object exData)
//        {
//            OnlinePaymentResponse onlinePaymentResponse = new OnlinePaymentResponse();
//            DataRow dataRow = paymentData as DataRow;
//            DataRow dataRow2 = operatorData as DataRow;
//            string text = dataRow["InitialSessionNumber"] as string;
//            string text2 = (dataRow["SessionNumber"] is DBNull) ? "" : (dataRow["SessionNumber"] as string);
//            int num = (int)dataRow["CyberplatOperatorID"];
//            int num2 = (int)dataRow["TerminalID"];
//            int num3 = (int)dataRow["StatusID"];
//            onlinePaymentResponse.StatusID = num3;
//            int num4 = (int)dataRow["ErrorCode"];
//            onlinePaymentResponse.ErrorCode = num4;
//            double num5 = BaseGateway.otof(dataRow["Amount"]);
//            double num6 = BaseGateway.otof(dataRow["AmountAll"]);
//            string text3 = dataRow2["Service"] as string;
//            string[] array = text3.Split(new string[]
//            {
//                ";"
//            }, StringSplitOptions.RemoveEmptyEntries);
//            string text4 = (dataRow2["OsmpFormatString"] is DBNull || text3 != "") ? "" : (dataRow2["OsmpFormatString"] as string);
//            string text5 = dataRow["Params"] as string;
//            string text6 = BaseGateway.FormatParameters(text5, text4);
//            Utils.StringList stringList = new Utils.StringList(text6, "\\n");
//            text5 = stringList.Strings + "\\n";
//            string text7 = (array.Length < 1) ? (dataRow2["RequestUrl"] as string) : array[0];
//            string text8 = (array.Length < 2) ? (dataRow2["PaymentUrl"] as string) : array[1];
//            string text9 = (array.Length < 3) ? (dataRow2["CheckUrl"] as string) : array[2];
//            int num7 = -1;
//            if (text2.Length > 0)
//            {
//                try
//                {
//                    num4 = 902;
//                    string subUrl = text9;
//                    num4 = -1;
//                    string request = CyberplatGateway.msgSESSION + text2 + "\r\n";
//                    string text10;
//                    CyberplatGateway.ErrorStatus errorStatus = this.TransactCyberplat(subUrl, request, out text10);
//                    if (errorStatus != CyberplatGateway.ErrorStatus.OK)
//                    {
//                        base.log(string.Concat(new object[]
//                        {
//                            "Status KeyID=",
//                            this.keyID,
//                            " (",
//                            text,
//                            ",",
//                            text2,
//                            ")=",
//                            errorStatus.ToString()
//                        }));
//                        throw new Exception("$");
//                    }
//                    string[] array2 = text10.Split(new string[]
//                    {
//                        "\r\n"
//                    }, StringSplitOptions.None);
//                    for (int i = 0; i < array2.Length; i++)
//                    {
//                        try
//                        {
//                            if (array2[i].StartsWith(CyberplatGateway.msgERROR))
//                            {
//                                num4 = int.Parse(array2[i].Substring(CyberplatGateway.msgERROR.Length));
//                            }
//                            if (array2[i].StartsWith(CyberplatGateway.msgRESULT) && array2[i].Length > CyberplatGateway.msgRESULT.Length)
//                            {
//                                num7 = int.Parse(array2[i].Substring(CyberplatGateway.msgRESULT.Length));
//                            }
//                        }
//                        catch (ThreadAbortException ex)
//                        {
//                            throw ex;
//                        }
//                        catch
//                        {
//                        }
//                    }
//                    base.log(string.Concat(new object[]
//                    {
//                        "Status KeyID=",
//                        this.keyID,
//                        " (",
//                        text,
//                        ",",
//                        text2,
//                        ")=",
//                        num7.ToString(),
//                        ",",
//                        num4.ToString()
//                    }));
//                    if (num4 == 11)
//                    {
//                        if (!(dataRow["PaymentDateTime"] is DBNull))
//                        {
//                            throw new Exception(string.Concat(new string[]
//                            {
//                                "Error 11 for status for payment with initial_session=",
//                                text,
//                                ", session=",
//                                text2,
//                                ", error=",
//                                num4.ToString(),
//                                "!"
//                            }));
//                        }
//                        num7 = 0;
//                    }
//                    if (num7 == -1)
//                    {
//                        throw new Exception(string.Concat(new string[]
//                        {
//                            "Error checking status for payment initial_session=",
//                            text,
//                            ", session=",
//                            text2,
//                            ", error=",
//                            num4.ToString(),
//                            "!"
//                        }));
//                    }
//                    if (num4 != 0 && num7 == 7)
//                    {
//                        num7 = 0;
//                    }
//                }
//                catch (Exception ex2)
//                {
//                    if (num4 == 11 || num4 == 902 || (DateTime)dataRow["StartDateTime"] + TimeSpan.FromDays(1.0) < DateTime.Now)
//                    {
//                        BaseGateway.PreprocessPaymentStatus.Invoke(num2, text, num4, 101, exData);
//                        onlinePaymentResponse.StatusID = 101;
//                        onlinePaymentResponse.ErrorCode = num4;
//                    }
//                    throw ex2;
//                }
//            }
//            if ((num3 == 103 || num3 == 104) && num7 < 2)
//            {
//                BaseGateway.PreprocessPaymentStatus.Invoke(num2, text, (int)dataRow["ErrorCode"], (num3 == 103) ? 102 : 100, exData);
//                onlinePaymentResponse.StatusID = ((num3 == 103) ? 102 : 100);
//                onlinePaymentResponse.ErrorCode = (int)dataRow["ErrorCode"];
//            }
//            else
//            {
//                if (num7 >= 2 && num7 != num3)
//                {
//                    BaseGateway.PreprocessPaymentStatus.Invoke(num2, text, (int)dataRow["ErrorCode"], num7, exData);
//                    onlinePaymentResponse.StatusID = num7;
//                    onlinePaymentResponse.ErrorCode = (int)dataRow["ErrorCode"];
//                }
//                if (num7 < 2)
//                {
//                    num3 = num7;
//                    text2 = BaseGateway.GenerateSessionNumber();
//                    string request = string.Concat(new string[]
//                    {
//                        CyberplatGateway.msgSESSION,
//                        text2,
//                        "\r\n",
//                        text5.Replace("\\n", "\r\n"),
//                        CyberplatGateway.msgAMOUNT,
//                        BaseGateway.ftos(num5),
//                        "\r\n",
//                        CyberplatGateway.msgAMOUNT_ALL,
//                        BaseGateway.ftos(num6),
//                        "\r\n",
//                        CyberplatGateway.msgCOMMENT,
//                        text,
//                        "-",
//                        num2.ToString(),
//                        "\r\n"
//                    });
//                    string subUrl = text7;
//                    string text10;
//                    CyberplatGateway.ErrorStatus errorStatus = this.TransactCyberplat(subUrl, request, out text10);
//                    if (errorStatus != CyberplatGateway.ErrorStatus.OK)
//                    {
//                        base.log(string.Concat(new object[]
//                        {
//                            "Request KeyID=",
//                            this.keyID,
//                            " (",
//                            text,
//                            ",",
//                            text2,
//                            ")=",
//                            errorStatus.ToString()
//                        }));
//                        throw new Exception("$");
//                    }
//                    string[] array2 = text10.Split(new string[]
//                    {
//                        "\r\n"
//                    }, StringSplitOptions.None);
//                    num4 = -1;
//                    for (int i = 0; i < array2.Length; i++)
//                    {
//                        if (array2[i].StartsWith(CyberplatGateway.msgERROR))
//                        {
//                            num4 = int.Parse(array2[i].Substring(CyberplatGateway.msgERROR.Length));
//                        }
//                        if (array2[i].StartsWith(CyberplatGateway.msgRESULT) && array2[i].Length > CyberplatGateway.msgRESULT.Length)
//                        {
//                            num7 = int.Parse(array2[i].Substring(CyberplatGateway.msgRESULT.Length));
//                        }
//                    }
//                    base.log(string.Concat(new object[]
//                    {
//                        "Request KeyID=",
//                        this.keyID,
//                        " (",
//                        text,
//                        ",",
//                        text2,
//                        ")=",
//                        num4.ToString()
//                    }));
//                    if (num4 == 21)
//                    {
//                        this.zeroBalance = true;
//                    }
//                    else
//                    {
//                        this.zeroBalance = false;
//                    }
//                    if (num4 > 0)
//                    {
//                        BaseGateway.PreprocessPaymentStatus.Invoke(num2, text, num4, 0, exData);
//                        onlinePaymentResponse.StatusID = 0;
//                        onlinePaymentResponse.ErrorCode = num4;
//                    }
//                    if (num4 == 0 && num7 == 0)
//                    {
//                        BaseGateway.PreprocessPayment.Invoke(num2, text, text2, DateTime.Now, exData);
//                        subUrl = text8;
//                        if (this.use_second_point && num5 == num6)
//                        {
//                            errorStatus = this.TransactCyberplat(subUrl, request, out text10, this.cyberplatAP_2, this.cyberplatOP_2, this.cyberplatSecretKey_2);
//                        }
//                        else
//                        {
//                            errorStatus = this.TransactCyberplat(subUrl, request, out text10);
//                        }
//                        if (errorStatus != CyberplatGateway.ErrorStatus.OK)
//                        {
//                            base.log(string.Concat(new object[]
//                            {
//                                "Payment KeyID=",
//                                this.keyID,
//                                " (",
//                                text,
//                                ",",
//                                text2,
//                                ")=",
//                                errorStatus.ToString()
//                            }));
//                            throw new Exception("$");
//                        }
//                        array2 = text10.Split(new string[]
//                        {
//                            "\r\n"
//                        }, StringSplitOptions.None);
//                        for (int i = 0; i < array2.Length; i++)
//                        {
//                            if (array2[i].StartsWith(CyberplatGateway.msgERROR))
//                            {
//                                num4 = int.Parse(array2[i].Substring(CyberplatGateway.msgERROR.Length));
//                            }
//                            if (array2[i].StartsWith(CyberplatGateway.msgRESULT) && array2[i].Length > CyberplatGateway.msgRESULT.Length)
//                            {
//                                num3 = int.Parse(array2[i].Substring(CyberplatGateway.msgRESULT.Length));
//                            }
//                        }
//                        if (num4 == 21)
//                        {
//                            this.zeroBalance = true;
//                        }
//                        else
//                        {
//                            this.zeroBalance = false;
//                        }
//                        if (num4 == 1 || num4 == 11)
//                        {
//                            BaseGateway.PreprocessPayment.Invoke(num2, text, "", DateTime.Now, exData);
//                        }
//                        base.log(string.Concat(new object[]
//                        {
//                            "Payment KeyID=",
//                            this.keyID,
//                            " (",
//                            text,
//                            ",",
//                            text2,
//                            ")=",
//                            num4.ToString()
//                        }));
//                    }
//                }
//            }
//            return onlinePaymentResponse;
//        }

//        public override string RequestBalance()
//        {
//            WebClient webClient = new WebClient();
//            webClient.Encoding = Encoding.GetEncoding(1251);
//            try
//            {
//                string text = string.Format("SD={0}\r\nAP={1}\r\nOP={2}\r\n", this.cyberplatSD, this.cyberplatAP, this.cyberplatOP);
//                text = this.cyberplatSecretKey.signText(text);
//                text = HttpUtility.UrlEncode(text);
//                text = "inputmessage=" + text;
//                text = webClient.UploadString(this.cyberplatProcessingUrl + "/cgi-bin/mts_espp/mtspay_rest.cgi", text);
//                text = this.cyberplatPublicKey.verifyText(text);
//                string[] array = text.Split(new string[]
//                {
//                    "\r\n"
//                }, StringSplitOptions.RemoveEmptyEntries);
//                string[] array2 = array;
//                for (int i = 0; i < array2.Length; i++)
//                {
//                    string text2 = array2[i];
//                    if (text2.ToLower().StartsWith("rest="))
//                    {
//                        try
//                        {
//                            text = BaseGateway.ftos(BaseGateway.otof(text2.Substring(text2.IndexOf('=') + 1)));
//                        }
//                        catch
//                        {
//                            text = text2.Substring(text2.IndexOf('=') + 1);
//                        }
//                        break;
//                    }
//                }
//                base.log(string.Concat(new object[]
//                {
//                    "GetBalance for KeyID =",
//                    base.KeyID,
//                    " OK: ",
//                    text
//                }));
//                base.Balance = text;
//            }
//            catch (Exception ex)
//            {
//                base.Balance = "";
//                base.log(string.Concat(new object[]
//                {
//                    "GetBalance for KeyID =",
//                    base.KeyID,
//                    " exception: ",
//                    ex.Message
//                }));
//            }
//            return base.Balance;
//        }

//        public override string ProcessOnlineCheck(NewPaymentData paymentData, object operatorData)
//        {
//            DataRow dataRow = operatorData as DataRow;
//            string text = dataRow["Service"] as string;
//            string[] array = text.Split(new string[]
//            {
//                ";"
//            }, StringSplitOptions.RemoveEmptyEntries);
//            string result = "";
//            CyberplatGateway.ErrorStatus errorStatus = this.TransactCyberplat((array.Length < 1) ? paymentData.RequestLocalPath : array[0], paymentData.MessageLines, out result);
//            base.log(string.Concat(new string[]
//            {
//                "Transit request is complete, (",
//                paymentData.RequestLocalPath,
//                ", ",
//                (paymentData.Comment.Length > 0) ? paymentData.Comment : paymentData.TerminalID.ToString(),
//                ")=",
//                errorStatus.ToString()
//            }));
//            if (errorStatus != CyberplatGateway.ErrorStatus.OK)
//            {
//                throw new Exception("Error SemiOnline Request.");
//            }
//            return result;
//        }

//        public override DataTable GetStatistics(DateTime dateFrom, DateTime dateTo)
//        {
//            DataTable dataTable = new DataTable();
//            string text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stat");
//            if (!Directory.Exists(text))
//            {
//                Directory.CreateDirectory(text);
//            }
//            string text2 = Path.Combine(text, string.Concat(new string[]
//            {
//                "file_stat",
//                base.GateProfileID.ToString(),
//                ".",
//                dateFrom.Date.ToString("yyyy-MM-dd"),
//                ".gz"
//            }));
//            base.log("Trying to get statistic file '" + text2.Substring(text.Length) + "'...");
//            byte[] array;
//            if (!this.TransactCyberplatStatistics(this.cyberplatStatUrl, dateFrom.Date, this.cyberplatSD, this.cyberplatSecretStatKey, this.cyberplatPublicKey, out array))
//            {
//                throw new Exception("TransactCyberplatStatistics failed");
//            }
//            dataTable.Columns.Add("TerminalID");
//            dataTable.Columns.Add("InitialSessionNumber");
//            dataTable.Columns.Add("Transaction");
//            dataTable.Columns.Add("PaymentDateTime");
//            dataTable.Columns.Add("AmountAll");
//            dataTable.Columns.Add("Amount");
//            dataTable.Columns.Add("CyberplatOperatorID");
//            dataTable.Columns.Add("Params");
//            dataTable.Columns.Add("GateId");
//            if (array.Length > 0)
//            {
//                int num = 0;
//                int num2 = 0;
//                File.WriteAllBytes(text2, array);
//                base.log("Received cyberplat statistics file '" + text2.Substring(text.Length) + "'");
//                SevenZip.Unzip(AppDomain.CurrentDomain.BaseDirectory + "\\7z.exe", "x -y", text2, text);
//                File.Delete(text2);
//                text2 = text2.Substring(0, text2.Length - Path.GetExtension(text2).Length);
//                string[] array2 = File.ReadAllLines(text2, Encoding.GetEncoding(1251));
//                int num3 = 0;
//                string[] array3 = array2;
//                for (int i = 0; i < array3.Length; i++)
//                {
//                    string text3 = array3[i];
//                    string[] array4 = text3.Split(new char[]
//                    {
//                        '|'
//                    });
//                    if (array4.Length >= 14)
//                    {
//                        try
//                        {
//                            int num4 = int.Parse(array4[8]);
//                            int num5 = int.Parse(array4[11]);
//                            if (num5 != 7 || num4 != 0)
//                            {
//                                goto IL_52F;
//                            }
//                            string text4 = array4[0];
//                            DateTime dateTime = DateTime.Parse(dateFrom.ToString("yyyy-MM-dd ") + array4[1]);
//                            int num6 = 0;
//                            string text5 = "";
//                            if (array4[6].Length > 0)
//                            {
//                                text5 = "Phone=\"" + array4[6] + "\"";
//                                if (array4[5].Length > 0)
//                                {
//                                    text5 += ", ";
//                                }
//                            }
//                            if (array4[5].Length > 0)
//                            {
//                                text5 = text5 + "Account=\"" + array4[5] + "\"";
//                            }
//                            double num7 = BaseGateway.otof(array4[7]);
//                            int num8 = int.Parse(array4[10]);
//                            string text6 = array4[12];
//                            double num9 = num7;
//                            try
//                            {
//                                num9 = BaseGateway.otof(array4[13]);
//                            }
//                            catch
//                            {
//                            }
//                            while (text4.Length < 20)
//                            {
//                                text4 = "0" + text4;
//                            }
//                            text4 = text4.Substring(0, 20);
//                            if (text6.Length > 20 && text6[20] == '-')
//                            {
//                                num6 = int.Parse(text6.Substring(21));
//                                text6 = text6.Substring(0, 20);
//                            }
//                            else if (text6.Length == 20)
//                            {
//                                num6 = int.Parse(array4[3]);
//                                text6 = text6.Substring(0, 20);
//                            }
//                            else
//                            {
//                                text6 = text4;
//                            }
//                            if (text5.Length > 256)
//                            {
//                                text5 = text5.Substring(0, 256);
//                            }
//                            num8 = this.ConvertOperatorID(num8);
//                            dataTable.Rows.Add(new object[]
//                            {
//                                num6,
//                                text6,
//                                text4,
//                                dateTime,
//                                num9,
//                                num7,
//                                num8,
//                                text5,
//                                base.GatewayID
//                            });
//                            num3 = 0;
//                            num2++;
//                        }
//                        catch (Exception ex)
//                        {
//                            num3++;
//                            if (num3 > 10)
//                            {
//                                base.log("ProcessCyberplatStatistics error - Can't parse line " + num.ToString() + " (trminating job)." + ex.ToString());
//                                throw ex;
//                            }
//                            base.log("ProcessCyberplatStatistics error - Can't parse line " + num.ToString() + ". " + ex.ToString());
//                        }
//                        num++;
//                    }
//                IL_52F:;
//                }
//            }
//            else
//            {
//                base.log("ProcessCyberplatStatistics zero length response file");
//            }
//            return dataTable;
//        }

//        private int ConvertOperatorID(int operatorID)
//        {
//            int result;
//            if (operatorID != 548)
//            {
//                switch (operatorID)
//                {
//                    case 714:
//                        result = 49;
//                        return result;
//                    case 716:
//                        result = 54;
//                        return result;
//                    case 717:
//                        result = 39;
//                        return result;
//                }
//                result = operatorID;
//            }
//            else
//            {
//                result = 38;
//            }
//            return result;
//        }

//        public virtual IGateway Clone()
//        {
//            return new CyberplatGateway(this);
//        }

//        public bool TransactCyberplatStatistics(string cyberRequestURL, DateTime date, int sd, IPrivKey secretKey, IPrivKey publicKey, out byte[] gzipFile)
//        {
//            bool flag = false;
//            gzipFile = new byte[0];
//            try
//            {
//                string text = string.Concat(new string[]
//                {
//                    "CLI_SERIAL=",
//                    sd.ToString(),
//                    "\r\nDATE=",
//                    date.Date.ToString("dd.MM.yyyy"),
//                    "\r\n"
//                });
//                string text2 = secretKey.signText(text);
//                text2 = HttpUtility.UrlEncode(text2);
//                text2 = "inputmessage=" + text2;
//                string text3 = new WebClient
//                {
//                    Encoding = Encoding.GetEncoding(1251)
//                }.UploadString(cyberRequestURL, text2);
//                string text4 = "CONTENT\r\n";
//                int num = text3.IndexOf(text4);
//                if (num >= 0)
//                {
//                    Encoding @default = Encoding.Default;
//                    gzipFile = @default.GetBytes(text3.Substring(num + text4.Length));
//                    text3 = text3.Substring(0, num);
//                }
//                text3 = publicKey.verifyText(text3);
//                string text5 = "ERROR=";
//                int num2 = text3.IndexOf(text5);
//                if (num2 < 0)
//                {
//                    throw new Exception("Не найдено поле ERROR, answer=" + text3);
//                }
//                num2 += text5.Length;
//                int num3 = int.Parse(text3.Substring(num2, text3.IndexOf("\r\n", num2) - num2));
//                if (num3 == 23)
//                {
//                    gzipFile = new byte[0];
//                    flag = true;
//                }
//                else if (num3 == 0 && num >= 0)
//                {
//                    string text6 = "SIZE=";
//                    int num4 = text3.IndexOf(text6);
//                    if (num4 < 0)
//                    {
//                        throw new Exception("Не найдено поле SIZE, answer=" + text3);
//                    }
//                    num4 += text6.Length;
//                    int num5 = int.Parse(text3.Substring(num4, text3.IndexOf("\r\n", num4) - num4));
//                    if (num5 != gzipFile.Length)
//                    {
//                        throw new Exception("Значение поля SIZE не соответствует длине файла, answer=" + text3);
//                    }
//                    string text7 = "DATE=";
//                    int num6 = text3.IndexOf(text7);
//                    if (num6 < 0)
//                    {
//                        throw new Exception("Не найдено поле DATE, answer=" + text3);
//                    }
//                    num6 += text7.Length;
//                    string a = text3.Substring(num6, text3.IndexOf("\r\n", num6) - num6);
//                    if (a != date.Date.ToString("dd.MM.yyyy"))
//                    {
//                        throw new Exception("Значение поля DATE не соответствует запрашиваемой дате '" + date.Date.ToString("dd-MM-yyyy") + "', answer=" + text3);
//                    }
//                    flag = true;
//                }
//            }
//            catch (IPrivException ex)
//            {
//                base.log("Ошибка ключей: " + ex.ToString() + " ");
//            }
//            catch (Exception ex2)
//            {
//                base.log(ex2.ToString());
//            }
//            if (!flag)
//            {
//                gzipFile = new byte[0];
//            }
//            return flag;
//        }

//        private CyberplatGateway.ErrorStatus TransactCyberplat(string subUrl, string request, out string response)
//        {
//            return this.TransactCyberplat(subUrl, request, out response, this.cyberplatAP, this.cyberplatOP, this.cyberplatSecretKey);
//        }

//        private CyberplatGateway.ErrorStatus TransactCyberplat(string subUrl, string request, out string response, int ap, int op, IPrivKey key)
//        {
//            CyberplatGateway.ErrorStatus result = CyberplatGateway.ErrorStatus.UnknownError;
//            int num;
//            lock (CyberplatGateway.tr_counterLock)
//            {
//                num = CyberplatGateway.tr_counter++;
//            }
//            response = "";
//            try
//            {
//                string requestUriString = this.cyberplatProcessingUrl + subUrl;
//                request = string.Concat(new string[]
//                {
//                    CyberplatGateway.msgAP,
//                    ap.ToString(),
//                    "\r\n",
//                    CyberplatGateway.msgOP,
//                    op.ToString(),
//                    "\r\n",
//                    CyberplatGateway.msgSD,
//                    this.cyberplatSD.ToString(),
//                    "\r\n",
//                    request
//                });
//                result = CyberplatGateway.ErrorStatus.ProcessingSecretKeyError;
//                if (this.detailLogEnabled)
//                {
//                    base.DetailLog("request: " + request);
//                }
//                request = key.signText(request);
//                result = CyberplatGateway.ErrorStatus.UnknownError;
//                Encoding encoding = Encoding.GetEncoding(1251);
//                request = HttpUtility.UrlEncode(request, encoding);
//                request = "inputmessage=" + request;
//                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
//                if (!string.IsNullOrEmpty(this.proxyUrl))
//                {
//                    WebProxy webProxy = new WebProxy(this.proxyUrl, true);
//                    if (!string.IsNullOrEmpty(this.proxyLogin))
//                    {
//                        webProxy.Credentials = new NetworkCredential(this.proxyLogin, this.proxyPassword);
//                    }
//                    httpWebRequest.Proxy = webProxy;
//                }
//                Stream stream = null;
//                WebResponse webResponse = null;
//                Stream stream2 = null;
//                StreamReader streamReader = null;
//                try
//                {
//                    httpWebRequest.Method = "POST";
//                    httpWebRequest.Timeout = 100000;
//                    httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
//                    httpWebRequest.ConnectionGroupName = num.ToString() + "req2cyber";
//                    byte[] bytes = encoding.GetBytes(request);
//                    httpWebRequest.ContentType = "application/x-www-form-urlencoded";
//                    httpWebRequest.ContentLength = (long)bytes.Length;
//                    httpWebRequest.KeepAlive = false;
//                    result = CyberplatGateway.ErrorStatus.CyberplatConnectionError;
//                    stream = httpWebRequest.GetRequestStream();
//                    stream.Write(bytes, 0, bytes.Length);
//                    stream.Close();
//                    stream = null;
//                    webResponse = httpWebRequest.GetResponse();
//                    stream2 = webResponse.GetResponseStream();
//                    streamReader = new StreamReader(stream2, encoding);
//                    response = streamReader.ReadToEnd();
//                    streamReader.Close();
//                    streamReader = null;
//                    stream2.Close();
//                    stream2 = null;
//                    webResponse.Close();
//                    webResponse = null;
//                }
//                catch (Exception ex)
//                {
//                    if (stream != null)
//                    {
//                        try
//                        {
//                            stream.Close();
//                        }
//                        catch
//                        {
//                        }
//                    }
//                    if (streamReader != null)
//                    {
//                        try
//                        {
//                            streamReader.Close();
//                        }
//                        catch
//                        {
//                        }
//                    }
//                    if (stream2 != null)
//                    {
//                        try
//                        {
//                            stream2.Close();
//                        }
//                        catch
//                        {
//                        }
//                    }
//                    if (webResponse != null)
//                    {
//                        try
//                        {
//                            webResponse.Close();
//                        }
//                        catch
//                        {
//                        }
//                    }
//                    throw ex;
//                }
//                result = CyberplatGateway.ErrorStatus.ProcessingPublicKeyError;
//                response = this.cyberplatPublicKey.verifyText(response);
//                if (this.detailLogEnabled)
//                {
//                    base.DetailLog("response: " + response);
//                }
//                result = CyberplatGateway.ErrorStatus.OK;
//            }
//            catch (Exception ex)
//            {
//                base.log("TransactCyberplatFailed:\n" + ex.ToString());
//            }
//            return result;
//        }


//    }


//}
