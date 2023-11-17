using Epicor.ServiceModel.Channels;

using Erp.Contracts;
using Erp.Proxy.BO;

using Ice.Core;
using Ice.Lib.Framework;

using System;

using UnlockCashGroup.Properties;

namespace UnlockCashGroup
{
    internal class Program
    {
        static Settings settings = Settings.Default;
        static Session epicorSession;
        static CashGrpImpl cashGrp;

        static string epicorUserID = settings.EpicorUserID;
        static string cashGroupID = settings.CashGroupID;

        static void Main(string[] args)
        {
            try
            {
                InitSession();
                UnlockCashGroup();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                epicorSession.Dispose();

                Console.WriteLine();
                Console.WriteLine("Press a key to exit...");
                Console.ReadKey();
            }
        }

        static void InitSession()
        {
            Console.WriteLine("Initializing Epicor Session");

            epicorSession = new Session(epicorUserID, settings.EpicorPassword, Session.LicenseType.Default, settings.EpicorConfigPath);

            cashGrp = WCFServiceSupport.CreateImpl<CashGrpImpl>(epicorSession, ImplBase<CashGrpSvcContract>.UriPath);

            Console.WriteLine("Epicor Session initialized");
            Console.WriteLine();
        }

        static void UnlockCashGroup()
        {
            try 
            {
                var list = cashGrp.GetList($"GroupID = '{cashGroupID}'", 0, 0, out _);

                if (list.CashGrpList.Rows.Count == 0)
                {
                    Console.WriteLine($"Cash Group {cashGroupID} does not exist");
                }

                var row = list.CashGrpList[0];
                var activeUserID = row.ActiveUserID;
                
                if (string.IsNullOrWhiteSpace(activeUserID)) 
                {
                    Console.WriteLine($"Cash Group {cashGroupID} is not locked");
                    return;
                }

                cashGrp.LeaveCashGrp(cashGroupID);

                list = cashGrp.GetList($"GroupID = '{cashGroupID}'", 0, 0, out _);
                row = list.CashGrpList[0];
                activeUserID = row.ActiveUserID;

                if (string.IsNullOrWhiteSpace(activeUserID))
                {
                    Console.WriteLine($"Cash Group {cashGroupID} unlocked successfully!");
                }
                else
                {
                    Console.WriteLine($"Unable to unlock Cash Group {cashGroupID}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
