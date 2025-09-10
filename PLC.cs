using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using Modbus.Data;
using Modbus.Device; // from NModbus4
using System.IO;
using Microsoft.Extensions.Configuration;
using log4net;
using log4net.Config;
using System.Reflection;
namespace PLCConnection
{
    public class PLC
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        public static string GetDMCNumber()
        {
            Logger.Info("GetDMCNumber");
            var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

            string plcIp = config.GetSection("PLC")["IpAddress"]!;//  "10.168.158.230"; // PLC IP address
            int port = Convert.ToInt16(config.GetSection("PLC")["Port"]);              // Modbus TCP port
            byte SlaveId = Convert.ToByte(config.GetSection("PLC")["SlaveId"]);

            ushort StartAddress = config.GetSection("PLC").GetValue<ushort>("StartAddress");
            ushort RegisterCount = config.GetSection("PLC").GetValue<ushort>("RegisterCount");
            //ushort StartAddress = Convert.ushort(config.GetSection("PLC")["StartAddress"]);
            //"StartAddress": 510,
            // "RegisterCount": 5


            string DMC = "";

            try
            {
                using (TcpClient client = new TcpClient(plcIp, port))
                {
                    // Create Modbus master
                    var modbusMaster = ModbusIpMaster.CreateIp(client);

                    byte slaveId = SlaveId;  // Unit ID (often 1)
                    ushort startAddress = StartAddress; // D510
                    ushort numRegisters = RegisterCount;   // D510–D514

                    // Read holding registers
                    ushort[] registers = modbusMaster.ReadHoldingRegisters(slaveId, startAddress, numRegisters);
                    byte[] bytes = new byte[registers.Length * 2];
                    Buffer.BlockCopy(registers, 0, bytes, 0, bytes.Length);

                    for (int i = 0; i < registers.Length; i++)
                    {
                        // Swap high/low bytes
                        bytes[i * 2] = (byte)(registers[i] & 0xFF);      // low
                        bytes[i * 2 + 1] = (byte)(registers[i] >> 8);         // high
                    }

                    DMC = Encoding.ASCII.GetString(bytes).TrimEnd('\0');

                    Logger.Info("DMC="+ DMC);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to PLC: " + ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Logger.Error(ex);
            }
            return DMC;
        }
    }
}
