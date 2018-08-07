using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SmartCalendar
{
    // https://developers.google.com/calendar/quickstart/dotnet
    // https://developers.google.com/calendar/concepts/events-calendars#recurring_events
    // https://developers.google.com/calendar/v3/reference/events

    class Program
    {
        static string[] scopes = { CalendarService.Scope.Calendar };
        static string ApplicationName = "SmartCalendar";

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            LoadConfigJson();

            ListEvents(service);

            Console.Read();
        }

        public static void LoadConfigJson()
        {
            if (File.Exists("EventConfig.json"))
            {
                using (StreamReader streamReader = new StreamReader("EventConfig.json"))
                {
                    string json = streamReader.ReadToEnd();
                    //EventConfig items = JsonConvert.DeserializeObject<EventConfig>(json);
                    //Event items = JsonConvert.DeserializeObject<Event>(json, new JsonSerializerSettings { DateFormatString = "yyyy-MM-ddTH:mm:ss" });
                    Event items = JsonConvert.DeserializeObject<Event>(json);
                }
            }
            
            //dynamic array = JsonConvert.DeserializeObject(json);
            //foreach(var item in array)
            //{
            //    Console.WriteLine("{0} {1}", item.temp, item.vcc);
            //}
        }

        public static void CreateEvent(CalendarService service, Event eventConfig)
        {
            Event newEvent = new Event
            {
                Summary = eventConfig.Summary, //"Daily Mobile 2",
                Location = eventConfig.Location, //"Um lugar qualquer",
                Start = new EventDateTime
                {
                    DateTime = new DateTime(2018, 8, 7, 10, 15, 0),
                    TimeZone = "America/Sao_Paulo"
                },
                End = new EventDateTime
                {
                    DateTime = new DateTime(2018, 8, 7, 10, 30, 0),
                    TimeZone = "America/Sao_Paulo"
                },
                Recurrence = new String[] {
                    "RRULE:FREQ=WEEKLY;UNTIL=20180817;BYDAY=MO,TU,WE,TH,FR"
                    //"RRULE:FREQ=WEEKLY;BYDAY=MO,TU,WE,TH,FR;COUNT=10",
                    //"EXDATE;VALUE=DATE:20180806,20180817"
                    //"RRULE:FREQ=WEEKLY;COUNT=5;BYDAY=TU,FR"
                },
                Attendees = new List<EventAttendee>()
                {
                    new EventAttendee { Email = "jgoldschmidt@gvdasa.com.br" }
                }
            };

            Event recurringEvent = service.Events.Insert(newEvent, "primary").Execute();
        }

        public static void ListEvents(CalendarService service)
        {
            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");

            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();

                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }

                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
        }
    }
}
