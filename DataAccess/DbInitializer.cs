﻿using DIS_Group10.DataAccess;
using DIS_Group10.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DIS_Group10.DataAccess
{
    public static class DbInitializer
    {
        static HttpClient httpClient;
        static string BASE_URL = "https://developer.nps.gov/api/v1";
        static string API_KEY = "VXU1saXu5sQvPJawH28GDeoVR8nT6C56vmxGiBFB";
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            getStates(context);
            getActivities(context);
            getParks(context);
        }

// Get Data From API:: Park Data :: /parks

        public static void getParks(ApplicationDbContext context)
        {

            if (context.Parks.Any())
            {
                return;
            }

            string uri = BASE_URL + "/parks?limit=50";
            string responsebody = "";
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Api-Key", API_KEY);
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.BaseAddress = new Uri(uri);

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(uri).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    responsebody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }

                if (!responsebody.Equals(""))
                {
                    JObject parsedResponse = JObject.Parse(responsebody);
                    JArray parks = (JArray)parsedResponse["data"];

                    foreach (JObject jpark in parks)
                    {
                        Park park = new Park
                        {
                            ID = (string)jpark["id"],
                            url = (string)jpark["url"],
                            fullName = (string)jpark["fullName"],
                            parkCode = (string)jpark["parkCode"],
                            description = (string)jpark["description"],
                        };
                        context.Parks.Add(park);
                        string[] states = ((string)jpark["states"]).Split(",");
                        foreach (string s in states)
                        {
                            State state = context.States.Where(c => c.ID == s).FirstOrDefault();
                            if (state != null)
                            {
                                StatePark statepark = new StatePark()
                                {
                                    state = state,
                                    park = park
                                };
                                context.StateParks.Add(statepark);
                                context.SaveChanges();
                            }
                        }
                        JArray activities = (JArray)jpark["activities"];
                        if (activities.Count != 0)
                        {
                            foreach (JObject jsonactivity in activities)
                            {
                                Activity activity = context.Activities.Where(c => c.ID == (string)jsonactivity["id"]).FirstOrDefault();
                                if (activity == null)
                                {
                                    activity = new Activity
                                    {
                                        ID = (string)jsonactivity["id"],
                                        name = (string)jsonactivity["name"]
                                    };
                                    context.Activities.Add(activity);
                                    context.SaveChanges();
                                }
                                ParkActivity parkactivity = new ParkActivity
                                {
                                    activity = activity,
                                    park = park
                                };
                                context.ParkActivities.Add(parkactivity);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

// Get Data From API:: Activity Data :: /activities

        public static void getActivities(ApplicationDbContext context)
        {
            if (context.Activities.Any())
            {
                return;
            }

            string uri = BASE_URL + "/activities?limit=50";
            string responsebody = "";

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Add("X-Api-Key", API_KEY);
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.BaseAddress = new Uri(uri);

            try
            {
                HttpResponseMessage response = httpClient.GetAsync(uri).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    responsebody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }

                if (!responsebody.Equals(""))
                {
                    JObject parsedResponse = JObject.Parse(responsebody);
                    JArray activities = (JArray)parsedResponse["data"];
                    foreach (JObject jsonactivity in activities)
                    {
                        Activity activity = new Activity
                        {
                            ID = (string)jsonactivity["id"],
                            name = (string)jsonactivity["name"]
                        };
                        context.Activities.Add(activity);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

// Park API Pulls State Code Only and not Name. Force Persisting State Code vs State Name in DB for Clean State Name Display

        public static void getStates(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            State[] statelist = new State[]
            {
                new State{ID="AL",name="Alabama"},
                new State{ID="AK",name="Alaska"},
                new State{ID="AZ",name="Arizona"},
                new State{ID="AR",name="Arkansas"},
                new State{ID="CA",name="California"},
                new State{ID="CO",name="Colorado"},
                new State{ID="DE",name="Delaware"},
                new State{ID="DC",name="District of Columbia"},
                new State{ID="FL",name="Florida"},
                new State{ID="GA",name="Georgia"},
                new State{ID="HI",name="Hawaii"},
                new State{ID="ID",name="Idaho"},
                new State{ID="IL",name="Illinois"},
                new State{ID="IN",name="Indiana"},
                new State{ID="IA",name="Iowa"},
                new State{ID="KS",name="Kansas"},
                new State{ID="KY",name="Kentucky"},
                new State{ID="LA",name="Louisiana"},
                new State{ID="ME",name="Maine"},
                new State{ID="MD",name="Maryland"},
                new State{ID="MA",name="Massachusetts"},
                new State{ID="MI",name="Michigan"},
                new State{ID="MN",name="Minnesota"},
                new State{ID="MS",name="Mississippi"},
                new State{ID="MO",name="Missouri"},
                new State{ID="MT",name="Montana"},
                new State{ID="NE",name="Nebraska"},
                new State{ID="NV",name="Nevada"},
                new State{ID="NH",name="New Hampshire"},
                new State{ID="NJ",name="New Jersey"},
                new State{ID="NM",name="New Mexico"},
                new State{ID="NY",name="New York"},
                new State{ID="NC",name="North Carolina"},
                new State{ID="ND",name="North Dakota"},
                new State{ID="OH",name="Ohio"},
                new State{ID="OK",name="Oklahoma"},
                new State{ID="OR",name="Oregon"},
                new State{ID="PA",name="Pennsylvania"},
                new State{ID="RI",name="Rhode Island"},
                new State{ID="SC",name="South Carolina"},
                new State{ID="SD",name="South Dakota"},
                new State{ID="TN",name="Tennessee"},
                new State{ID="TX",name="Texas"},
                new State{ID="UT",name="Utah"},
                new State{ID="VT",name="Vermont"},
                new State{ID="VA",name="Virginia"},
                new State{ID="WA",name="Washington"},
                new State{ID="WV",name="West Virginia"},
                new State{ID="WI",name="Wisconsin"},
                new State{ID="WY",name="Wyoming"},
            };

            if (!context.States.Any())
            {
                foreach (State st in statelist)
                {
                    context.States.Add(st);
                }
                context.SaveChanges();
            }
        }

    }
}