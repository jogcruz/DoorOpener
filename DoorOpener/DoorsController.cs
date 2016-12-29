using DoorOpener.Data;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DoorOpener
{
    public class DoorsController : ApiController 
    {
        // GET api/doors 
        public IHttpActionResult Get()
        {
            var service = new DoorsService();
            //service.TestRelay();
            //return new string[] { "value1", "value2" };

            // Get all logs
            //var alllogs = Log.LoadLogs();
    
            return Ok(service.GetDoors());
        }

        // GET api/doors/5 
        public string Get(int id)
        {
            return "value";
        }

        // GET api/doors/latestevents
        [Route("api/doors/latestevents")]
        [HttpGet]
        public IHttpActionResult GetLatestEvents()
        {
            // Get latest Logs
            var latest = Log.GetLatestLogs(5);
            if (latest != null && latest.Count > 0)
            {
                return Ok(latest);
            }
            else
            {
                return NotFound();
            }
            
        }

        [Route("api/doors/events")]
        [HttpGet]
        public IHttpActionResult GetEvents()
        {
            return Ok();
        }

        // GET api/doors/events
        [Route("api/doors/events")]
        [HttpGet]
        public IHttpActionResult GetEvents(string from, string to, int count, int page)
        {
            // Get latest Logs
            var events = Log.GetLogs(from, to, count, page);
            if (events != null && events.logs.Count > 0)
            {
                return Ok(events);
            }
            else
            {
                return NotFound();
            }

        }

        // POST api/doors 
        public void Post([FromBody]string value)
        {
           
        }

        // PUT api/doors/5 
        public void Put(int id, [FromBody]string value)
        {
            
        }

        // DELETE api/doors/5 
        public void Delete(int id)
        {
        } 


        [Route("api/toggle/{id}")]
        [HttpPut]
        public IHttpActionResult Toggle(int id)
        {
            Console.WriteLine("api/toggle/{id}");
            string ip = GetClientIp();
            Console.WriteLine("IP:" + ip);
            if (IsPermitted(ip))
            {
                var service = new DoorsService();
                Console.WriteLine("service.PushButton(id)");
                Door door = service.PushButton(id);

                // Add a log
                Log.AddLog(id, door.laststatetime, door.name, door.status, ip);
                return Ok(door);
            }
            else
            {
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.BadRequest, "Operation not permitted"));
            }
            
        }

        [Route("api/doorstatus/{id}")]
        [HttpGet]
        public IHttpActionResult GetDoorStatus(int id)
        {
            var service = new DoorsService();
            return Ok(service.GetDoorStatus(id));
        }




        private string GetClientIp()
        {
            HttpRequestMessage request = Request;
            // Web-hosting
            //if (request.Properties.ContainsKey("MS_HttpContext"))
            //{
            //    HttpContextWrapper ctx =
            //        (HttpContextWrapper)request.Properties["MS_HttpContext"];
            //    if (ctx != null)
            //    {
            //        return ctx.Request.UserHostAddress;
            //    }
            //}

            //// Self-hosting
            //if (request.Properties.ContainsKey(RemoteEndpointMessage))
            //{
            //    RemoteEndpointMessageProperty remoteEndpoint =
            //        (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessage];
            //    if (remoteEndpoint != null)
            //    {
            //        return remoteEndpoint.Address;
            //    }
            //}

            // Self-hosting using Owin
            if (request.Properties.ContainsKey("MS_OwinContext"))
            {
                OwinContext owinContext = (OwinContext)request.Properties["MS_OwinContext"];
                if (owinContext != null)
                {
                    return owinContext.Request.RemoteIpAddress;
                }
            }

            return "";
        }

        private bool IsPermitted(string ip)
        {
            return ip.Equals("::1") || ip.Equals("192.168.1.126") || DoorOpener.Data.User.UserExist(ip);
            //return DoorOpener.Data.User.UserExist(ip);
        }

    }
}
