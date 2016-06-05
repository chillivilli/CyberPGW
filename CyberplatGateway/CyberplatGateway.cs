//#define EMULATION
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Cache;
using System.Web;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Gateways.Utils;

namespace Gateways 
{
    public class CyberplatGateway : BaseGateway, IGateway, IGateOnlinePayment
    {
        protected bool detailLogEnabled = true;

        private string cyberplatProcessingUrl = "";
        private int cyberplatAP = 0;
        private int cyberplatOP = 0;
        private int cyberplatSD = 0;
        private org.CyberPlat.IPrivKey cyberplatSecretKey = null;
        private org.CyberPlat.IPrivKey cyberplatSecretStatKey = null;
        private org.CyberPlat.IPrivKey cyberplatPublicKey = null;
        private uint serial = 904291;
        private string testUrl = "/cgi-bin/es/es_pay_check.cgi";
        private string testPhone = "9166731169";
        private string proxyUrl, proxyLogin, proxyPassword;


        private string cyberplatStatUrl = "";

        //key number for answer signing
        private string cyberplatKeyNum = "";

        #region STATIC_ATTRIBUTES
        private static string msgAP = "AP=";
        private static string msgOP = "OP=";
        private static string msgSD = "SD=";
        private static string msgCOMMENT = "COMMENT=";
        private static string msgSESSION = "SESSION=";
        private static string msgNUMBER = "NUMBER=";
        private static string msgACCOUNT = "ACCOUNT=";
        private static string msgAMOUNT = "AMOUNT=";
        private static string msgAMOUNT_ALL = "AMOUNT_ALL=";
        private static string msgERROR = "ERROR=";
        private static string msgRESULT = "RESULT=";
        private static string msgREQ_TYPE1 = "REQ_TYPE=1";
        private static string msgCYBERERROR = "CYBERERROR=";
        private static string msgCYBERRESULT = "CYBERRESULT=";
        private static string msgNEXTPAYMENT = "NEXTPAYMENT";
        private static string msgURL = "URL=";


        private const string msgAcceptKeys = "ACCEPT_KEYS=";
        private const string msgDate = "DATE=";
        private const string msgTermId = "TERM_ID=";
        private const string msgPaytool = "PAY_TOOL=";

        // Для протокола загрузки/выгрузки файлов через предпроцессинг
        private static string msgFILENAME = "FILENAME=";
        private static string msgOFFSET = "OFFSET=";
        private static string msgSIZE = "SIZE=";
        private static string msgCONTENT = "CONTENT";
        private static string msgDOWNLOAD = "DOWNLOAD=";
        private static string msgUPLOAD = "UPLOAD=";
        #endregion

        private static Object tr_counterLock = new Object();
        private static int tr_counter = 0;

        enum ErrorStatus
        {
            OK = 0,
            BadRequest = 2048,
            ErrorParsingKeySerial = 2049,
            KeySerialNotFound = 2050,
            WrongRequestSign = 2051,
            ProcessingSecretKeyError = 2052,
            CyberplatConnectionError = 2053,
            ProcessingPublicKeyError = 2054,
            InternalProcessingSecretKeyError = 2055,
            OperatorNotFound = 2056,
            UnknownError = 4096
        }

        public CyberplatGateway()
        {
            Balance = "";
        }

        public CyberplatGateway(CyberplatGateway cyberplatGateway)
        {
            this.cyberplatProcessingUrl = cyberplatGateway.cyberplatProcessingUrl;
            this.cyberplatAP = cyberplatGateway.cyberplatAP;
            this.cyberplatOP = cyberplatGateway.cyberplatOP;
            this.cyberplatSD = cyberplatGateway.cyberplatSD;
            this.cyberplatSecretKey = cyberplatGateway.cyberplatSecretKey;
            this.cyberplatPublicKey = cyberplatGateway.cyberplatPublicKey;
            this.cyberplatSD = cyberplatGateway.cyberplatSD;
            this.cyberplatSecretStatKey = cyberplatGateway.cyberplatSecretStatKey;
            this.cyberplatPublicKey = cyberplatGateway.cyberplatPublicKey;
            this.cyberplatStatUrl = cyberplatGateway.cyberplatStatUrl;
            this.paySystemKeyID = cyberplatGateway.paySystemKeyID;

            this.serial = cyberplatGateway.serial;
            this.testUrl = cyberplatGateway.testUrl;
            this.testPhone = cyberplatGateway.testPhone;

            this.proxyUrl = cyberplatGateway.proxyUrl;
            this.proxyLogin = cyberplatGateway.proxyLogin;
            this.proxyPassword= cyberplatGateway.proxyPassword;

            this.GateProfileID = cyberplatGateway.GateProfileID;

            this.showAmountAll = cyberplatGateway.showAmountAll;

            this.Copy(cyberplatGateway);
        }

        ~CyberplatGateway()
        {
            if (cyberplatSecretKey != null)
                cyberplatSecretKey.closeKey();
            if (cyberplatSecretStatKey != null)
                cyberplatSecretStatKey.closeKey();
            if (cyberplatPublicKey != null)
                cyberplatPublicKey.closeKey();
            if (cyberplatSecretStatKey != null)
                cyberplatSecretStatKey.closeKey();
        }
        
        /// <summary>
        /// Инициализация состояния шлюза, ключи урлы и тд
        /// </summary>
        /// <param name="data">Данные в xml формате, по шаблону InitializeTemplate</param>
        public void Initialize(string data)
        {
            try
            {
                log("CyberplatGateway::Initialize, GateProfileID=" + GateProfileID.ToString());

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(data);

                cyberplatProcessingUrl = xml.DocumentElement["server_url"].InnerText;
                cyberplatAP = int.Parse(xml.DocumentElement["ap"].InnerText);
                cyberplatOP = int.Parse(xml.DocumentElement["op"].InnerText);
                cyberplatSD = int.Parse(xml.DocumentElement["sd"].InnerText);
                paySystemKeyID = cyberplatSD.ToString();                

                cyberplatSecretKey = org.CyberPlat.IPriv.openSecretKey(xml.DocumentElement["secret_key"].InnerText, xml.DocumentElement["password"].InnerText);                
                cyberplatPublicKey = org.CyberPlat.IPriv.openPublicKey(xml.DocumentElement["public_key"].InnerText, uint.Parse(xml.DocumentElement["serial"].InnerText));

                cyberplatKeyNum = xml.DocumentElement["skey_num"].InnerText;

                try
                {
                    proxyUrl = xml.DocumentElement["proxy_url"].InnerText;
                    proxyLogin = xml.DocumentElement["proxy_login"].InnerText;
                    proxyPassword = xml.DocumentElement["proxy_password"].InnerText;
                    log("CyberplatGateway::Initialize, proxy parameters OK");
                }
                catch
                {
                    log("CyberplatGateway::Initialize, proxy disabled");
                }

                if ((cyberplatAP == 0) || (cyberplatProcessingUrl == ""))
                    throw new Exception("Error! One or more parameters not set");

                try
                {
                    cyberplatStatUrl = xml.DocumentElement["stat_url"].InnerText;
                    cyberplatSecretStatKey = org.CyberPlat.IPriv.openSecretKey(xml.DocumentElement["stat_secret_key"].InnerText, xml.DocumentElement["stat_password"].InnerText);
                }
                catch (Exception ex)
                {
                    log("CyberplatGateway::Initialize, stat params exception:" + ex.ToString());
                }


                try
                {
                    serial = uint.Parse(xml.DocumentElement["serial"].InnerText);
                    testUrl = xml.DocumentElement["test_url"].InnerText;
                    testPhone = xml.DocumentElement["test_phone"].InnerText;
                }
                catch { }

            }
            catch (Exception ex)
            {
                log("CyberplatGateway::Initialize exception: " + ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// Проверка параметров
        /// </summary>
        /// <returns></returns>
        public string CheckSettings()
        {
            ErrorStatus errorStatus = ErrorStatus.UnknownError;

            string response = "No response.";
            try
            {
                string msc = ((int)((DateTime.Now - DateTime.Now.Date).TotalMilliseconds)).ToString();
                while (msc.Length < 12)
                    msc = "0" + msc;
                msc = msc.Substring(msc.Length - 12, 12);

                string requestString = string.Format("SD={0}\r\nAP={1}\r\nOP={2}\r\nSESSION={3}\r\nNUMBER={4}\r\nAMOUNT=10.00\r\nAMOUNT_ALL=10.00\r\n{5}",
                    cyberplatSD, cyberplatAP, cyberplatOP, DateTime.Now.ToString("ddMMyyyy") + msc, testPhone, testPhone != "1111111111" ? "REQ_TYPE=1\r\nCOMMENT=MONITORING_CHECK\r\n" : "");

                errorStatus = ErrorStatus.ProcessingSecretKeyError;

                requestString = cyberplatSecretKey.signText(requestString);

                errorStatus = ErrorStatus.UnknownError;

                requestString = HttpUtility.UrlEncode(requestString);
                requestString = "inputmessage=" + requestString;

                WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.GetEncoding(1251);

                errorStatus = ErrorStatus.CyberplatConnectionError;

                string responseString = webClient.UploadString(cyberplatProcessingUrl + testUrl, requestString);

                errorStatus = ErrorStatus.ProcessingPublicKeyError;

                responseString = cyberplatPublicKey.verifyText(responseString);

                response = responseString;
                errorStatus = ErrorStatus.InternalProcessingSecretKeyError;

                string keyPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"Keys");

                if (File.Exists(keyPath + "\\internal.keyInfo"))
                {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(File.ReadAllText(keyPath + "\\internal.keyInfo", Encoding.GetEncoding(1251)));
                    org.CyberPlat.IPrivKey internalKey = org.CyberPlat.IPriv.openSecretKey(xml.DocumentElement["secret_key"].InnerText, xml.DocumentElement["password"].InnerText);

                    responseString = internalKey.signText(responseString);
                    errorStatus = ErrorStatus.OK;
                    internalKey.closeKey();
                }

                try
                {
                    response += "\r\nПроверка статистики Киберплат, RESULT=";

                    byte[] gzipFile;
                    bool result = TransactCyberplatStatistics(cyberplatStatUrl, new DateTime(1990, 1, 1), this.cyberplatSD, cyberplatSecretStatKey, cyberplatPublicKey, out gzipFile);
                    if (!result)
                        throw new Exception("Не удалось получить данные Cyberplat,\r\nпроверьте правильность ключей и связи с сервером Cyberplat.");

                    response += "OK";
                }
                catch(Exception ex)
                {
                    response += ex.Message;
                }
            }
            catch (org.CyberPlat.IPrivException e)
            {
                response = e.ToString();
            }
            catch
            {
            }

            return errorStatus.ToString() + "\r\n" + response;
        }

        /// <summary>
        /// Оффлайн обработка платежа
        /// </summary>
        /// <param name="paymentData"></param>
        /// <param name="operatorData"></param>
        public void ProcessPayment(object paymentData, object operatorData, object exData)
        {
            DataRow paymentRow = paymentData as DataRow;
            string initial_session = (paymentRow["InitialSessionNumber"] as string);
            try
            {
                ProcessOfflinePayment(paymentData, operatorData, exData);
            }
            catch (Exception ex)
            {
                log(string.Format("ProcessOfflinePayment KeyID={2}(initial_session={0}) exception: {1}", initial_session, ex.Message, keyID));
            }
        }

        /// <summary>
        /// Онлайн проведение платежа
        /// </summary>
        public OnlinePaymentResponse ProcessOnlinePayment(DataRow paymentRow, object operatorData)
        {
            OnlinePaymentResponse response = new OnlinePaymentResponse();
            string initial_session = paymentRow["InitialSessionNumber"] as string;
            try
            {
                response.StatusID = (int)paymentRow["StatusID"];
                response.ErrorCode = (int)paymentRow["ErrorCode"];

                if ((response.StatusID < 7) || (response.StatusID == 103) || (response.StatusID == 104))
                    response = ProcessOfflinePayment(paymentRow, operatorData, null);
            }
            catch (Exception ex)
            {
                log(string.Format("ProcessOnlinePayment KeyID={2} (initial_session={0}) exception: {1}", initial_session, ex.Message, keyID));
            }

            return response;
        }

        /// <summary>
        /// Оффлайн обработка платежа
        /// </summary>
        /// <param name="paymentData"></param>
        /// <param name="operatorData"></param>
        public OnlinePaymentResponse ProcessOfflinePayment(object paymentData, object operatorData, object exData)
        {
            OnlinePaymentResponse response = new OnlinePaymentResponse();
            DataRow paymentRow = paymentData as DataRow;
            DataRow operatorRow = operatorData as DataRow;

            string initial_session = (paymentRow["InitialSessionNumber"] as string);
            string session = (paymentRow["SessionNumber"] is DBNull) ? "" : (paymentRow["SessionNumber"] as string);
            int cyberplatOperatorID = (int)paymentRow["CyberplatOperatorID"];
            int ap = (int)paymentRow["TerminalID"];
            int status = (int)paymentRow["StatusID"];
            response.StatusID = status;
            int errorCode = (int)paymentRow["ErrorCode"];
            response.ErrorCode = errorCode;
            string paymentParams = paymentRow["Params"] as string;
            double _amount = otof(paymentRow["Amount"]);
            double _amountAll = otof(paymentRow["AmountAll"]);
            string service = operatorRow["Service"] as string;
            string[] serviceUrls = service.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            


            string operatorFormatString = operatorRow["OsmpFormatString"] is DBNull || service != "" ? "" : operatorRow["OsmpFormatString"] as string;
            string requestUrl = serviceUrls.Length < 1 ? operatorRow["RequestUrl"] as string : serviceUrls[0];
            string paymentUrl = serviceUrls.Length < 2 ? operatorRow["PaymentUrl"] as string : serviceUrls[1];
            string statusUrl = serviceUrls.Length < 3 ? operatorRow["CheckUrl"] as string : serviceUrls[2];

            string request;
            string answer;

            // Убираем ненужные киберу данные...
            StringList stringList = new StringList(paymentParams, "\\n");
            stringList.Remove("CARD_NUMBER");
            stringList.Remove("POINT_FROM");
            stringList.Remove("SESSION_FROM");
            stringList.Remove("BANK_NAME");
            paymentParams = stringList.Strings + "\\n";

            int result = -1;

            if (session.Length > 0)
            {
                try
                {
                    string url;
                    errorCode = 902;
                    url = statusUrl;
                    errorCode = -1;

                    request = msgSESSION + session + "\r\n";
                    ErrorStatus errorStatus = TransactCyberplat(url, request, out answer);
                    if (errorStatus != ErrorStatus.OK)
                    {
                        log("Status KeyID=" + keyID + " (" + initial_session + "," + session + ")=" + errorStatus.ToString());
                        throw new Exception("$");
                    }
                    string[] messageLines = answer.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                    for (int i = 0; i < messageLines.Length; i++)
                    {
                        try
                        {
                            if (messageLines[i].StartsWith(msgERROR))
                                errorCode = int.Parse(messageLines[i].Substring(msgERROR.Length));
                            if (messageLines[i].StartsWith(msgRESULT) && messageLines[i].Length > msgRESULT.Length)
                                result = int.Parse(messageLines[i].Substring(msgRESULT.Length));
                        }
                        catch (ThreadAbortException e)
                        {
                            throw e;
                        }
                        catch
                        {
                        }
                    }

                    log("Status KeyID=" + keyID + " (" + initial_session + "," + session + ")=" + result.ToString() + "," + errorCode.ToString());

                    if (errorCode == 11)
                    {
                        if (paymentRow["PaymentDateTime"] is DBNull)
                            result = 0;
                        else
                            throw new Exception("Error 11 for status for payment with initial_session=" + initial_session + ", session=" + session + ", error=" + errorCode.ToString() + "!");
                    }
                    if ((result == -1)/* || (errorCode == 24) || (errorCode == 30)*/)
                        throw new Exception("Error checking status for payment initial_session=" + initial_session + ", session=" + session + ", error=" + errorCode.ToString() + "!");

                    if ((errorCode != 0) && (result == 7))
                        result = 0;
                }
                catch (Exception ex)
                {
                    if ((errorCode == 11) || (errorCode == 902) || ((DateTime)paymentRow["StartDateTime"] + TimeSpan.FromDays(1) < DateTime.Now))
                    {
                        PreprocessPaymentStatus(ap, initial_session, errorCode, 101, exData);
                        response.StatusID = 101;
                        response.ErrorCode = errorCode;
                    }
                    throw ex;
                }
            }

            if (((status == 103) || (status == 104)) && (result < 2))
            {
                PreprocessPaymentStatus(ap, initial_session, (int)paymentRow["ErrorCode"], (status == 103) ? 102 : 100, exData);
                response.StatusID = (status == 103) ? 102 : 100;
                response.ErrorCode = (int)paymentRow["ErrorCode"];
            }
            else
            {
                if ((result >= 2) && (result != status))
                {
                    PreprocessPaymentStatus(ap, initial_session, (int)paymentRow["ErrorCode"], result, exData);
                    response.StatusID = result;
                    response.ErrorCode = (int)paymentRow["ErrorCode"];
                    //if (result == 7)
                    //    ServerStatistics.AddProcessingPayment();
                }
                if (result < 2)
                {
                    status = result;

                    session = GenerateSessionNumber();

                    DateTime paymentDate = (DateTime)paymentRow["InitializeDateTime"];
                    //int amount = (int)(((double)cwd.row["Amount"] + 0.00001) * 100);

                    request =
                        msgSESSION + session + "\r\n" +
                        msgAcceptKeys + cyberplatKeyNum + "\r\n" +
                        paymentParams.Replace("\\n", "\r\n") +
                        msgDate + paymentDate.ToUniversalTime().ToString("dd.MM.yyyy HH:mm:ss") + "\r\n" +
                        msgPaytool + 0 + "\r\n" +
                        msgTermId + ap.ToString() + "\r\n" +
                        msgAMOUNT + ftos(_amount) + "\r\n" +
                        msgAMOUNT_ALL + ftos(_amountAll) + "\r\n" +
                        msgCOMMENT + initial_session + "-" + ap.ToString() + "\r\n";

                    string url = requestUrl;
                    ErrorStatus errorStatus = TransactCyberplat(url, request, out answer);
                    if (errorStatus != ErrorStatus.OK)
                    {
                        log("Request KeyID=" + keyID + " (" + initial_session + "," + session + ")=" + errorStatus.ToString());
                        throw new Exception("$");
                    }
                    string[] messageLines = answer.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                    errorCode = -1;
                    for (int i = 0; i < messageLines.Length; i++)
                    {
                        if (messageLines[i].StartsWith(msgERROR))
                            errorCode = int.Parse(messageLines[i].Substring(msgERROR.Length));
                        if (messageLines[i].StartsWith(msgRESULT) && messageLines[i].Length > msgRESULT.Length)
                            result = int.Parse(messageLines[i].Substring(msgRESULT.Length));
                    }

                    log("Request KeyID=" + keyID + " (" + initial_session + "," + session + ")=" + errorCode.ToString());

                    if (errorCode == 21)
                        zeroBalance = true;
                    else
                        zeroBalance = false;

                    if (errorCode > 0)
                    {
                        PreprocessPaymentStatus(ap, initial_session, errorCode, 0, exData);
                        response.StatusID = 0;
                        response.ErrorCode = errorCode;
                    }

                    if ((errorCode == 0) && (result == 0))
                    {
                        PreprocessPayment(ap, initial_session, session, DateTime.Now, exData);

                        url = paymentUrl;
                        errorStatus = TransactCyberplat(url, request, out answer);
                        if (errorStatus != ErrorStatus.OK)
                        {
                            log("Payment KeyID=" + keyID + " (" + initial_session + "," + session + ")=" + errorStatus.ToString());
                            throw new Exception("$");
                        }

                        messageLines = answer.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                        for (int i = 0; i < messageLines.Length; i++)
                        {
                            if (messageLines[i].StartsWith(msgERROR))
                                errorCode = int.Parse(messageLines[i].Substring(msgERROR.Length));
                            if (messageLines[i].StartsWith(msgRESULT) && messageLines[i].Length > msgRESULT.Length)
                                status = int.Parse(messageLines[i].Substring(msgRESULT.Length));
                        }

                        if (errorCode == 21)
                            zeroBalance = true;
                        else
                            zeroBalance = false;

                        if ((errorCode == 1) || (errorCode == 11)) // session alredy exists or stupid 11 cyber error (no session in DB)
                        {
                            PreprocessPayment(ap, initial_session, "", DateTime.Now, exData);
                        }

                        log("Payment KeyID=" + keyID + " (" + initial_session + "," + session + ")=" + errorCode.ToString());
                    }
                }
            }
            return response;
        }

        /// <summary>
        /// Получение балланса
        /// </summary>
        /// <returns></returns>
        public override string RequestBalance()
        {
            WebClient webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.GetEncoding(1251);
            try
            {
                string restText = String.Format("SD={0}\r\nAP={1}\r\nOP={2}\r\n{3}{4}\r\n", cyberplatSD, cyberplatAP, cyberplatOP, msgAcceptKeys, cyberplatKeyNum);
                
                string url = cyberplatProcessingUrl + "/cgi-bin/mts_espp/mtspay_rest.cgi";

                DetailLog("url: " + url);
                DetailLog("request: " + restText);

                restText = cyberplatSecretKey.signText(restText);

                restText = HttpUtility.UrlEncode(restText);
                restText = "inputmessage=" + restText;


                
                restText = webClient.UploadString(url, restText);

                restText = cyberplatPublicKey.verifyText(restText);


                string[] lines = restText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string line in lines)
                {
                    if (line.ToLower().StartsWith("rest="))
                    {
                        try
                        {
                            restText = ftos(otof(line.Substring(line.IndexOf('=') + 1)));
                        }
                        catch
                        {
                            restText = line.Substring(line.IndexOf('=') + 1);
                        }

                        break;
                    }
                }

                log("GetBalance for KeyID =" + KeyID + " OK: " + restText);
                Balance = restText;
            }
            catch (Exception ex)
            {
                Balance = "";
                log("GetBalance for KeyID =" + KeyID + " exception: " + ex.Message);
            }

            return Balance;
        }

        /// <summary>
        /// Онлайн проверка платежа
        /// </summary>
        public override string ProcessOnlineCheck(NewPaymentData paymentData, object operatorData)
        {
            DataRow operatorRow = operatorData as DataRow;
            string service = operatorRow["Service"] as string;
            string[] serviceUrls = service.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            string responseString = "";

            string request = paymentData.MessageLines;
            request = request + "\r\n" + msgAcceptKeys + cyberplatKeyNum + "\r\n"
                    + msgPaytool + 0 + "\r\n"
                    + msgDate + paymentData.InitializeDateTime.ToUniversalTime().ToString("dd.MM.yyyy HH:mm:ss") + "\r\n"
                    + msgTermId + paymentData.TerminalID + "\r\n";
                

            ErrorStatus errorStatus = TransactCyberplat(serviceUrls.Length < 1 ? paymentData.RequestLocalPath : serviceUrls[0], request, out responseString);
            log("Transit request is complete, (" + paymentData.RequestLocalPath + ", " + ((paymentData.Comment.Length) > 0 ? paymentData.Comment : paymentData.TerminalID.ToString()) + ")=" + errorStatus.ToString());
            if (errorStatus != ErrorStatus.OK)
                throw new Exception("Error SemiOnline Request.");

            return responseString;
        }

        public override DataTable GetStatistics(DateTime dateFrom, DateTime dateTo)
        {
            DataTable table = new DataTable();
            string statPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "stat");
            if (!Directory.Exists(statPath))
                Directory.CreateDirectory(statPath);

            string fileName = Path.Combine(statPath, "file_stat" + GateProfileID.ToString() + "." + dateFrom.Date.ToString("yyyy-MM-dd") + ".gz");
            log("Trying to get statistic file '" + fileName.Substring(statPath.Length) + "'...");
            byte[] gzipFile;

            bool result = TransactCyberplatStatistics(cyberplatStatUrl, dateFrom.Date, cyberplatSD, cyberplatSecretStatKey, cyberplatPublicKey, out gzipFile);
            if (!result)
            {
                throw new Exception("TransactCyberplatStatistics failed");
            }

            table.Columns.Add("TerminalID");
            table.Columns.Add("InitialSessionNumber");
            table.Columns.Add("Transaction");
            table.Columns.Add("PaymentDateTime");
            table.Columns.Add("AmountAll");
            table.Columns.Add("Amount");
            table.Columns.Add("CyberplatOperatorID");
            table.Columns.Add("Params");
            table.Columns.Add("GateId");

            if (gzipFile.Length > 0)
            {
                int lineNumber = 0;
                int linesProcessed = 0;

                File.WriteAllBytes(fileName, gzipFile);
                log("Received cyberplat statistics file '" + fileName.Substring(statPath.Length) + "'");

                PPS.MyTools.SevenZip.Unzip(System.AppDomain.CurrentDomain.BaseDirectory + "\\7z.exe", "x -y", fileName, statPath);

                File.Delete(fileName);

                fileName = fileName.Substring(0, fileName.Length - Path.GetExtension(fileName).Length);

                string[] lines = File.ReadAllLines(fileName, Encoding.GetEncoding(1251));
                int lineOK = 0;

                foreach (string line in lines)
                {
                    string[] data = line.Split('|');

                    if (data.Length < 14)
                        continue;

                    try
                    {
                        int error = int.Parse(data[8]);
                        int status = int.Parse(data[11]);

                        if ((status != 7) || (error != 0))
                            continue;

                        string transaction = data[0];
                        DateTime time = DateTime.Parse(dateFrom.ToString("yyyy-MM-dd ") + data[1]);
                        int terminalID = 0;
                        string param = "";
                        if (data[6].Length > 0)
                        {
                            param = "Phone=\"" + data[6] + "\"";
                            if (data[5].Length > 0)
                                param += ", ";
                        }

                        if (data[5].Length > 0)
                            param += "Account=\"" + data[5] + "\"";

                        double amount = otof(data[7]);
                        int operatorID = int.Parse(data[10]);
                        string session = data[12];
                        double amountAll = amount;
                        try { amountAll = otof(data[13]); }
                        catch { }

                        while (transaction.Length < 20)
                            transaction = "0" + transaction;
                        transaction = transaction.Substring(0, 20);

                        if ((session.Length > 20) && (session[20] == '-'))
                        {
                            terminalID = int.Parse(session.Substring(21));
                            session = session.Substring(0, 20);
                        }
                        else if (session.Length == 20)
                        {
                            terminalID = int.Parse(data[3]);
                            session = session.Substring(0, 20);
                        }
                        else
                            session = transaction;

                        if (param.Length > 256)
                            param = param.Substring(0, 256);

                        operatorID = ConvertOperatorID(operatorID);

                        table.Rows.Add(new object[] { terminalID, session, transaction, time, amountAll, amount, operatorID, param, GatewayID });

                        lineOK = 0;
                        linesProcessed++;
                    }
                    catch (Exception ex)
                    {
                        lineOK++;
                        if (lineOK > 10)
                        {
                            log("ProcessCyberplatStatistics error - Can't parse line " + lineNumber.ToString() + " (trminating job)." + ex.ToString());
                            throw ex;
                        }
                        else
                            log("ProcessCyberplatStatistics error - Can't parse line " + lineNumber.ToString() + ". " + ex.ToString());
                    }

                    lineNumber++;
                }
            }
            else
            {
                log("ProcessCyberplatStatistics zero length response file");
            }

            return table;
        }
        
        int ConvertOperatorID(int operatorID)
        {
            switch (operatorID)
            {
                case 548: return 38;    
                case 714: return 49;
                case 716: return 54;
                case 717: return 39;
                default: return operatorID;
            }
        }

        /// <summary>
        /// Возвращает копию объекта
        /// </summary>
        public virtual IGateway Clone()
        {
            return new CyberplatGateway(this);
        }

        public bool TransactCyberplatStatistics(string cyberRequestURL, DateTime date, int sd, org.CyberPlat.IPrivKey secretKey, org.CyberPlat.IPrivKey publicKey, out byte[] gzipFile)
        {
            bool result = false;
            gzipFile = new byte[] { };

            try
            {
                string request = "CLI_SERIAL=" + sd.ToString() + "\r\nDATE=" + date.ToUniversalTime().Date.ToString("dd.MM.yyyy") + "\r\n";
                string s = secretKey.signText(request);

                s = HttpUtility.UrlEncode(s);
                s = "inputmessage=" + s;
                
                WebClient webClient = new WebClient();
                webClient.Encoding = System.Text.Encoding.GetEncoding(1251);
                
                string answer = webClient.UploadString(cyberRequestURL, s);

                string CONTENT = "CONTENT\r\n";
                int fileIndex = answer.IndexOf(CONTENT);
                if (fileIndex >= 0)
                {
                    Encoding encoding = Encoding.Default;
                    gzipFile = encoding.GetBytes(answer.Substring(fileIndex + CONTENT.Length));

                    answer = answer.Substring(0, fileIndex);
                }

                answer = publicKey.verifyText(answer);

                string ERROR = "ERROR=";
                int errorIndex = answer.IndexOf(ERROR);
                if (errorIndex < 0)
                    throw new Exception("Не найдено поле ERROR, answer=" + answer);

                errorIndex += ERROR.Length;
                int error = int.Parse(answer.Substring(errorIndex, answer.IndexOf("\r\n", errorIndex) - errorIndex));

                if (error == 23) // 23 - нет данных
                {
                    gzipFile = new byte[] { };
                    result = true;
                }
                else if ((error == 0) && (fileIndex >= 0))
                {

                    string SIZE = "SIZE=";
                    int sizeIndex = answer.IndexOf(SIZE);
                    if (sizeIndex < 0)
                        throw new Exception("Не найдено поле SIZE, answer=" + answer);

                    sizeIndex += SIZE.Length;
                    int size = int.Parse(answer.Substring(sizeIndex, answer.IndexOf("\r\n", sizeIndex) - sizeIndex));
                    if (size != gzipFile.Length)
                        throw new Exception("Значение поля SIZE не соответствует длине файла, answer=" + answer);


                    string DATE = "DATE=";
                    int dateIndex = answer.IndexOf(DATE);
                    if (dateIndex < 0)
                        throw new Exception("Не найдено поле DATE, answer=" + answer);

                    dateIndex += DATE.Length;
                    string date1 = answer.Substring(dateIndex, answer.IndexOf("\r\n", dateIndex) - dateIndex);
                    if (date1 != date.Date.ToString("dd.MM.yyyy"))
                        throw new Exception("Значение поля DATE не соответствует запрашиваемой дате '" + date.Date.ToString("dd-MM-yyyy") + "', answer=" + answer);

                    result = true;
                }
            }
            catch (org.CyberPlat.IPrivException err)
            {
                log("Ошибка ключей: " + err.ToString() + " ");
            }
            catch (Exception ex)
            {
                log(ex.ToString());
            }

            if (!result)
                gzipFile = new byte[] { };

            return result;
        }


        private ErrorStatus TransactCyberplat(string subUrl, string request, out string response)
        {
            ErrorStatus errorStatus = ErrorStatus.UnknownError;

            int tr_no;
            lock (tr_counterLock)
            {
                tr_no = tr_counter++;
            }

            response = "";
            try
            {
                string url = cyberplatProcessingUrl + subUrl;

                request = msgAP + cyberplatAP.ToString() + "\r\n" +
                    msgOP + cyberplatOP.ToString() + "\r\n" +
                    msgSD + cyberplatSD.ToString() + "\r\n" + request;

                DetailLog("url: " + url);

                errorStatus = ErrorStatus.ProcessingSecretKeyError;

                if (detailLogEnabled)
                    DetailLog("request: " + request);

                request = cyberplatSecretKey.signText(request);

                errorStatus = ErrorStatus.UnknownError;

                Encoding enc = Encoding.GetEncoding(1251);
                request = HttpUtility.UrlEncode(request, enc);
                request = "inputmessage=" + request;

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url); //+"?"+request); (GET)
                if (!String.IsNullOrEmpty(proxyUrl))
                {
                    WebProxy proxy = new WebProxy(proxyUrl, true);
                    if (!String.IsNullOrEmpty(proxyLogin))
                        proxy.Credentials = new NetworkCredential(proxyLogin, proxyPassword);
                    req.Proxy = proxy;
                }

                Stream requestStream = null;
                WebResponse res = null;
                Stream receiveStream = null;
                StreamReader readStream = null;
                try
                {
                    req.Method = "POST";
                    req.Timeout = 100000;
                    //req.TransferEncoding = 
                    req.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                    req.ConnectionGroupName = tr_no.ToString() + "req2cyber";
                    byte[] data = enc.GetBytes(request);
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.ContentLength = data.Length;
                    req.KeepAlive = false;

                    errorStatus = ErrorStatus.CyberplatConnectionError;

                    requestStream = req.GetRequestStream();
                    requestStream.Write(data, 0, data.Length);
                    requestStream.Close();
                    requestStream = null;

                    res = req.GetResponse();

                    receiveStream = res.GetResponseStream();

                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    readStream = new StreamReader(receiveStream, enc);

                    response = readStream.ReadToEnd();

                    readStream.Close(); readStream = null;
                    receiveStream.Close(); receiveStream = null;
                    res.Close(); res = null;
                }
                catch (Exception ex)
                {
                    if (requestStream != null)
                    {
                        try { requestStream.Close(); }
                        catch { };
                    }
                    if (readStream != null)
                    {
                        try { readStream.Close(); }
                        catch { };
                    }
                    if (receiveStream != null)
                    {
                        try { receiveStream.Close(); }
                        catch { };
                    }
                    if (res != null)
                    {
                        try { res.Close(); }
                        catch { };
                    }

                    throw ex;
                }
                errorStatus = ErrorStatus.ProcessingPublicKeyError;
                response = cyberplatPublicKey.verifyText(response);

                if (detailLogEnabled)
                    DetailLog("response: " + response);

                errorStatus = ErrorStatus.OK;
            }
            catch (Exception ex)
            {
                log("TransactCyberplatFailed:\n" + ex.ToString());
            }
            return errorStatus;
        }
        
    }

  
}
