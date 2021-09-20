using cAstroCalc;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoSerialLink
{
  //  public class TelescopeTask {

		//public System.Threading.Tasks.TaskStatus taskStatus { get; set; }
  //      public eCalcSyncTaskTypes eCalcSyncTaskType { get; set; }
  //  }

	public enum eCalcSyncTaskTypes
	{
        Wait = 0,
        Slew = 1,
        Follow = 2,

	}

    public enum eTaskStatus
    {
        Nothing = 0,
        WaitingForProceed = 1,
        Running=2,

    }

    public class ArduinoStepMotorController
    {

        static SerialPort mySerialPort;

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Program");
        //public static bool RunningTaskSlew = false;
        //public static bool FollowingObjectTask = false;

        public Dictionary<eCalcSyncTaskTypes, eTaskStatus> TelescopeStepMotorTasks = new Dictionary<eCalcSyncTaskTypes, eTaskStatus>();

        public ArduinoStepMotorController(string portName)
        {
            Console.WriteLine($"ArduinoStepMotorController serial link starting on port: {portName}...");

            TelescopeStepMotorTasks.Add(eCalcSyncTaskTypes.Wait, eTaskStatus.Nothing);
            TelescopeStepMotorTasks.Add(eCalcSyncTaskTypes.Slew, eTaskStatus.Nothing);
            TelescopeStepMotorTasks.Add(eCalcSyncTaskTypes.Follow, eTaskStatus.Nothing);

            //Connecting to port:
            try
			{
				mySerialPort = new SerialPort(portName);
				mySerialPort.BaudRate = 9600;
				mySerialPort.Parity = Parity.None;
				mySerialPort.StopBits = StopBits.One;
				mySerialPort.DataBits = 8;
				mySerialPort.Handshake = Handshake.None;
				mySerialPort.Open();
				mySerialPort.Write("Connection with ArduinoStepMotorController OK.");
                mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedFromArduinoStepMotor);



            }
			catch (Exception ex)
			{
                log.Error(ex);
				//throw;

			}
        }

		private void DataReceivedFromArduinoStepMotor(object sender, SerialDataReceivedEventArgs e)
		{
            SerialPort _stelariumVirtualPort = (SerialPort)sender;
            if (!_stelariumVirtualPort.IsOpen) 
            { 
            return; 
            }
            int bytes = _stelariumVirtualPort.BytesToRead;
            byte[] buffer = new byte[bytes];
            _stelariumVirtualPort.Read(buffer, 0, bytes);
            string dataFromTelescope = Encoding.UTF8.GetString(buffer);
            Console.WriteLine($"DataReceived:{dataFromTelescope}");
            //TOOD: parsing data
            //TODO: pokud budou příkazy o natočení hotovy, pak :
            if (dataFromTelescope.Contains("RunningTaskSlewFinished")) {
                //RunningTaskSlew = false;
                TelescopeStepMotorTasks[eCalcSyncTaskTypes.Slew] = eTaskStatus.Nothing;
            }
            if (dataFromTelescope.Contains("Following"))
            {
                //RunningTaskSlew = false;
                TelescopeStepMotorTasks[eCalcSyncTaskTypes.Follow] = eTaskStatus.Running;
            }

        }

        /// <summary>
        /// Smyčka neustále nastavující uhly pro MotorArdino:
        /// </summary>
        /// <param name="userLatitude"></param>
        /// <param name="userLongtitude"></param>
        /// <param name="zone"></param>
        /// <param name="dst"></param>
        /// <param name="ra"></param>
        /// <param name="dec"></param>
        public void SlewToObject(Double userLatitude, Double userLongtitude, int zone, int dst , Double ra , Double dec) {


			try
			{
				cAstroCalc.cBasicAstro cBasicAstroData = new cAstroCalc.cBasicAstro(userLatitude, userLongtitude, zone, dst);
				ALT_AZIM_Values aLT_AZIM_Values = cBasicAstroData.az_al(DateTime.Now, ra, dec);
				String stepMottorCmd = $"AZ:{Math.Round((Double)aLT_AZIM_Values.Azim, 4)}|ALT:{Math.Round((Double)aLT_AZIM_Values.ALt, 4)}";
				mySerialPort.WriteLine(stepMottorCmd);
				//TODO: zde by se mělo asi čekat, až se příkaz dokončí...
                TelescopeStepMotorTasks[eCalcSyncTaskTypes.Slew] = eTaskStatus.Running;
                Console.WriteLine(stepMottorCmd);
				log.Debug(stepMottorCmd);
			}
			catch (Exception ex)
			{
                log.Error(ex);
				//throw; //zatimi nebudu nějak dál šířit...
			}

        }

        public void FollowObject(Double userLatitude, Double userLongtitude, int zone, int dst, Double ra, Double dec)
        {

            try
            {
                
                while (TelescopeStepMotorTasks[eCalcSyncTaskTypes.Follow] == eTaskStatus.Running)
                {
                    cAstroCalc.cBasicAstro cBasicAstroData = new cAstroCalc.cBasicAstro(userLatitude, userLongtitude, zone, dst);
                    ALT_AZIM_Values aLT_AZIM_Values = cBasicAstroData.az_al(DateTime.Now, ra, dec);
                    String stepMottorCmd = $"AZ:{Math.Round((Double)aLT_AZIM_Values.Azim, 4)}|ALT:{Math.Round((Double)aLT_AZIM_Values.ALt, 4)}";
                    mySerialPort.WriteLine(stepMottorCmd);
                    //TODO: zde by se mělo asi čekat, až se příkaz dokončí...
                    
                    Console.WriteLine(stepMottorCmd);
                    log.Debug(stepMottorCmd);
                    Thread.Sleep(1000);
                    TelescopeStepMotorTasks[eCalcSyncTaskTypes.Follow] = eTaskStatus.Running;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                //throw;
            }
        }

    }
}
