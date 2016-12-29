using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PiSharp.LibGpio;
using PiSharp.LibGpio.Entities;
using System.IO;
using Newtonsoft.Json;
using DoorOpener.Data;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace DoorOpener
{
    class DoorsService
    {
        public enum DoorStatus
        {
            open = 1,
            closed = 0
        };

        public Door PushButton(int id)
        {
            Door door = new Door(id);
            try
            {
                if (door != null && door.id > 0)
                {
                    var initialStatus = GetDoorStatus(id);
                    var nextStatus = GetNextFinalStatus(initialStatus);
                    if (!initialStatus.Equals(door.status))
                    {
                        // Add an error log
                        Log.AddLog(id, DateTime.Now, door.name, "Unexpected status: Expected status-" + door.status + ", Real status-" + nextStatus, "Unknow");
                    }


                    LibGpio.Gpio.TestMode = false;
                    LibGpio.Gpio.SetupChannel((BroadcomPinNumber)door.relaypin, Direction.Output);
                    LibGpio.Gpio.OutputValue((BroadcomPinNumber)door.relaypin, true);
                    System.Threading.Thread.Sleep(2000);
                    LibGpio.Gpio.OutputValue((BroadcomPinNumber)door.relaypin, false);
                    System.Threading.Thread.Sleep(10000);
                    //LibGpio.Gpio.CloseChannel((BroadcomPinNumber)door.relaypin);

                    var tmpStatus = "";
                    for (int i = 0; i < 20; i++)
                    {
                        Console.WriteLine("GetDoorStatus " + i.ToString());
                        tmpStatus = GetDoorStatus(id);
                        if (tmpStatus.Equals(nextStatus))
                        {
                            break;
                        }
                        System.Threading.Thread.Sleep(2000);
                    }

                    if (!tmpStatus.Equals(nextStatus))
                    {
                        // Add an error log
                        Log.AddLog(id, DateTime.Now, door.name, "Error final status: Initial status-" + initialStatus + ", Final status-" + tmpStatus, "Unknow");
                        door.inconsistent = true;
                    }

                    door.status = tmpStatus;
                    door.laststatetime = DateTime.Now;
                    door.PersistDoor(true);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("error: " + e.Message);
            }
            return door;
        }


        public List<Door> GetDoors()
        {                     
            Door door = new Door();
            return door.GetDoorsFromBD();            
        }

        public string GetDoorStatus(int id)
        {
            bool result = false;
            Door door = new Door(id);
            if (door != null && door.id > 0)
            {
                //LibGpio.Gpio.TestMode = false;
                LibGpio.Gpio.SetupChannel((BroadcomPinNumber)door.statuspin, Direction.Input);
                result = LibGpio.Gpio.ReadValue((BroadcomPinNumber)door.statuspin);
            }
            return Enum.GetName(typeof(DoorStatus), result);
        }

        public string GetNextFinalStatus(string currentStatus)
        {
            string nextStatus = "";
            if (currentStatus.Equals("open"))
            {
                nextStatus = "closed";
            }
            else if (currentStatus.Equals("closed"))
            {
                nextStatus = "open";
            }
            return nextStatus;
        }

        private void WriteToConsole(string message)
        {
            var writeToConsole = Convert.ToBoolean(ConfigurationManager.AppSettings["DebugToConsole"]);
            if (writeToConsole) {
                Console.WriteLine(message);
            }
            
        }


    }
}
