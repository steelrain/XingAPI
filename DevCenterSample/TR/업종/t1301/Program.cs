﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XingAPINet;

namespace t1301
{
    class Program
    {
        static void Main(string[] args)
        {
            bool useDemoServer = true;
            bool loadFromDB = false;

            Program pg = new Program();

            if (args.Length == 1)
            {
                if (args[0] == "hts")
                {
                    useDemoServer = false;
                }
                else if (args[0] == "db")
                {
                    loadFromDB = true;
                }
            }

            pg.Main(useDemoServer, loadFromDB);
        }

        void Main(bool useDemoServer, bool loadFromDB)
        {
            SqliteExtension.UseSqlite("test.sqlite");

            LoginInfo user = GetUserInfo(useDemoServer);

            using (XingClient xing = new XingClient(useDemoServer))
            {
                if (xing.ConnectWithLogin(user) == false)
                {
                    Console.WriteLine(xing.ErrorMessage);
                    return;
                }

                if (loadFromDB == true)
                {
                    var multiBlock = XQt1301.ReadFromDB();

                    if (multiBlock.OutBlock.IsValidData == false)
                    {
                        return;
                    }

                    multiBlock.OutBlock.Dump(Console.Out, DumpOutputType.Inline80Cols);

                    Console.WriteLine($"# of items: {multiBlock.OutBlock1.Length}");
                    foreach (var item in multiBlock.OutBlock1)
                    {
                        item.Dump(Console.Out, DumpOutputType.Inline80Cols);
                    }
                }
                else if (useDemoServer)
                {
                    var multiBlock = XQt1301.Get(Stock.SHCODE.KOSPI.현대건설,
                         starttime: "0900", endtime: "1530");

                    if (multiBlock.OutBlock.IsValidData == false)
                    {
                        return;
                    }

                    multiBlock.OutBlock.Dump(Console.Out, DumpOutputType.Inline80Cols);
                    multiBlock.OutBlock.WriteToDB(replace: true);

                    Console.WriteLine($"# of items: {multiBlock.OutBlock1.Length}");
                    foreach (var item in multiBlock.OutBlock1)
                    {
                        item.Dump(Console.Out, DumpOutputType.Inline);
                    }

                    multiBlock.OutBlock1.WriteToDB(replace: true);
                }
                else
                {
                    using (XQt1301 query = new XQt1301())
                    {
                        var inBlock = new XQt1301InBlock
                        {
                            shcode = Stock.SHCODE.KOSPI.현대건설,
                            starttime = "0900",
                            endtime = "1530",
                        };

                        while (true)
                        {
                            query.SetBlock(inBlock);
                            if (query.Request() < 0)
                            {
                                Console.WriteLine("Failed to send request");
                                break;
                            }

                            var outBlock = query.GetBlock();
                            if (outBlock.IsValidData == true)
                            {
                                outBlock.Dump(Console.Out, DumpOutputType.Inline);
                            }
                            else
                            {
                                Console.WriteLine($"Invalid: {outBlock.InvalidReason}");
                                break;
                            }

                            foreach (var item in query.GetBlock1s())
                            {
                                item.Dump(Console.Out, DumpOutputType.Inline80Cols);
                            }

                            inBlock.CopyValueFromBlock(outBlock);
                            if (string.IsNullOrEmpty(outBlock.cts_time) == true)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private LoginInfo GetUserInfo(bool useDemoServer)
        {
            string fileName = (useDemoServer == true) ? "ebest.demo.txt" : "ebest.hts.txt";
            string filePath = $@"d:\settings\{fileName}";

            return LoginInfo.FromEncryptedFile(filePath);
        }
    }
}
